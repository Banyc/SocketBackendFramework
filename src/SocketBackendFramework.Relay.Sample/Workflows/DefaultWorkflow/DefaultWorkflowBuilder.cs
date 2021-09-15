using SocketBackendFramework.Relay.Models.Workflows;
using SocketBackendFramework.Relay.Pipeline;
using SocketBackendFramework.Relay.Pipeline.Middlewares.ControllersMapper;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Controllers;
using SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow.DefaultPipelineDomain.Models;
using SocketBackendFramework.Relay.Workflows;

namespace SocketBackendFramework.Relay.Sample.Workflows.DefaultWorkflow
{
    public class DefaultWorkflowBuilder : WorkflowBuilder
    {
        public DefaultWorkflowBuilder(WorkflowConfig config) : base(config)
        {
        }

        protected override void ConfigurateWorkflow(WorkflowConfig config)
        {
            // build controllers mapper
            ControllersMapper<DefaultMiddlewareContext> defaultControllersMapper = new();
            // build pipeline domain
            DefaultPipelineDomainBuilder defaultPipelineDomainBuilder = new(config.GetPipelineDomainConfig("default"));
            PipelineDomain<DefaultMiddlewareContext> defaultPipelineDomain = defaultPipelineDomainBuilder.Build();
            // add controllers mappers to corresponding pipeline
            defaultPipelineDomain.Pipeline.Use(defaultControllersMapper);
            // add pipeline domain to workflow
            base.AddPipelineDomain(defaultPipelineDomain);

            // build controllers
            EchoController echoController = new(defaultPipelineDomain.Pipeline);
            GreetController greetController = new(defaultPipelineDomain.Pipeline);
            NoReplyController noReplyController = new();
            // add controllers to controllers mapper
            defaultControllersMapper.AddController(echoController);
            defaultControllersMapper.AddController(noReplyController);
            defaultControllersMapper.AddController(greetController);
        }
    }
}
