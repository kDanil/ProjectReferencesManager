using Castle.MicroKernel.Registration;
using Castle.Windsor;

namespace ProjectReferencesManager.Tools
{
    public class ToolsContainer
    {
        private readonly WindsorContainer container;

        public ToolsContainer()
        {
            this.container = new WindsorContainer();

            this.Install();
        }

        public IMainWindowViewModel MainWindowViewModel
        {
            get
            {
                return this.container.Resolve<IMainWindowViewModel>();
            }
        }

        private void Install()
        {
            this.container.Register(Component.For<IMainWindowViewModel>().ImplementedBy<MainWindowViewModel>());
            this.container.Register(Component.For<IProjectFileReader>().ImplementedBy<ProjectFileReader>());
            this.container.Register(Component.For<ICopyingManager>().ImplementedBy<CopyingManager>());
            this.container.Register(Component.For<ISolutionFileOpener>().ImplementedBy<SolutionFileOpener>());
            this.container.Register(Component.For<IProjectCollectionsModifier>().ImplementedBy<ProjectCollectionsModifier>());
            this.container.Register(Component.For<ISolutionLoader>().ImplementedBy<SolutionLoader>());
            this.container.Register(Component.For<IProjectsChangesManager>().ImplementedBy<ProjectsChangesManager>());
            this.container.Register(Component.For<IReferencesModifier>().ImplementedBy<ReferencesModifier>());
            this.container.Register(Component.For<IUserInteraction>().ImplementedBy<UserInteraction>());
        }
    }
}