using JetBrains.ProjectModel;
using JetBrains.ReSharper.UnitTestExplorer;

namespace Machine.Specifications.ReSharperRunner.Presentation
{
  internal class BehaviorElement : FieldElement
  {
    public BehaviorElement(IUnitTestProvider provider,
                           // ReSharper disable SuggestBaseTypeForParameter
                           ContextElement context,
                           // ReSharper restore SuggestBaseTypeForParameter
                           IProjectModelElement project,
                           string declaringTypeName,
                           string fieldName,
                           bool isIgnored)
      : base(provider, context, project, declaringTypeName, fieldName, isIgnored || context.IsExplicit)
    {
    }

    public ContextElement Context
    {
      get { return (ContextElement) Parent; }
    }

    public override string GetTitlePrefix()
    {
      return "behaves like";
    }

    public override string GetKind()
    {
      return "Behavior";
    }

    public override string GetHighlighterAttributeId()
    {
      // HACK: This relies on an implementation detail, but allows us to show the "container" icon, although this
      // element technically has no children when the FileExplorer has explored the assembly.
      return "ReSharper Unit Test Element Container";
    }
  }
}