﻿using System;
using System.Collections.Generic;

using Machine.Container.Model;

namespace Machine.Container.Services
{
  public interface IContainerInfrastructureFactory
  {
    IActivatorResolver CreateDependencyResolver();
    IInstanceTrackingPolicy CreateInstanceTrackingPolicy();
  }
}
