using System;
using System.Collections.Generic;
using System.Threading;

using Machine.Container.Model;
using Machine.Container.Plugins;
using Machine.Core.Utility;

namespace Machine.Container.Services.Impl
{
  public class ObjectInstances : IObjectInstances
  {
    private readonly ReaderWriterLock _lock = new ReaderWriterLock();
    private readonly Dictionary<object, ResolvedServiceEntry> _map = new Dictionary<object, ResolvedServiceEntry>();
    private readonly IListenerInvoker _listenerInvoker;

    public ObjectInstances(IListenerInvoker listenerInvoker)
    {
      _listenerInvoker = listenerInvoker;
    }

    public void Remember(ResolvedServiceEntry entry, object instance)
    {
      using (RWLock.AsReader((_lock)))
      {
        if (_map.ContainsKey(instance))
        {
          if (!_map[instance].Equals(entry))
          {
            throw new InvalidOperationException("Already have instance for: " + entry);
          }
        }
        else
        {
          _lock.UpgradeToWriterLock(Timeout.Infinite);
          _map[instance] = entry;
          _listenerInvoker.InstanceCreated(entry, instance);
          entry.IncrementActiveInstances();
        }
      }
    }

    public void Release(IResolutionServices services, object instance)
    {
      using (RWLock.AsWriter(_lock))
      {
        if (!_map.ContainsKey(instance))
        {
          throw new ServiceContainerException("Attempt to release instances NOT created by the container: " + instance);
        }
        _listenerInvoker.InstanceReleased(_map[instance], instance);
        _map[instance].DecrementActiveInstances();
        _map.Remove(instance);
      }
    }
  }
}