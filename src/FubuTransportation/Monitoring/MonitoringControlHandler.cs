using System;
using System.Threading.Tasks;
using FubuTransportation.Configuration;

namespace FubuTransportation.Monitoring
{
    // Mostly tested through PersistentTaskController and/or integration tests
    public class MonitoringControlHandler
    {
        private readonly ChannelGraph _graph;
        private readonly IPersistentTaskController _controller;

        public MonitoringControlHandler(ChannelGraph graph, IPersistentTaskController controller)
        {
            _graph = graph;
            _controller = controller;
        }

        public Task<TaskHealthResponse> Handle(TaskHealthRequest request)
        {
            return _controller.CheckStatusOfOwnedTasks().ContinueWith(t => {
                if (t.IsFaulted)
                {
                    return TaskHealthResponse.ErrorFor(request.Subjects);
                }

                var response = t.Result;
                response.AddMissingSubjects(request.Subjects);

                return response;
            });
        }

        public Task<TaskDeactivationResponse> Handle(TaskDeactivation deactivation)
        {
            return
                _controller.Deactivate(deactivation.Subject)
                    .ContinueWith(
                        t => new TaskDeactivationResponse
                        {
                            Subject = deactivation.Subject, 
                            Success = t.Result
                        });
        }

        public Task<TakeOwnershipResponse> Handle(TakeOwnershipRequest request)
        {
            return _controller.TakeOwnership(request.Subject).ContinueWith(t => new TakeOwnershipResponse
            {
                NodeId = _graph.NodeId,
                Status = t.Result,
                Subject = request.Subject
            });
        }
    }

    public class TaskDeactivationResponse
    {
        public Uri Subject { get; set; }
        public bool Success { get; set; }
    }
}