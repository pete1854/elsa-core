using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Elsa.Extensions;
using Elsa.Models;
using Elsa.Services.Models;
using Microsoft.Extensions.DependencyInjection;
using Open.Linq.AsyncExtensions;

namespace Elsa.Services
{
    public class WorkflowRegistry : IWorkflowRegistry
    {
        private readonly IServiceProvider _serviceProvider;

        public WorkflowRegistry(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<Workflow>> GetWorkflowsAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var providers = scope.ServiceProvider.GetServices<IWorkflowProvider>();
            var tasks = await Task.WhenAll(providers.Select(x => x.GetWorkflowsAsync(cancellationToken)));
            return tasks.SelectMany(x => x).ToList();
        }

        public async Task<Workflow?> GetWorkflowAsync(
            string id,
            VersionOptions version,
            CancellationToken cancellationToken)
        {
            var workflows = await GetWorkflowsAsync(cancellationToken).ToList();

            return workflows
                .Where(x => x.WorkflowDefinitionId == id)
                .OrderByDescending(x => x.Version)
                .WithVersion(version)
                .FirstOrDefault();
        }
    }
}