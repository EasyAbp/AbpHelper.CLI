namespace AbpHelper.Steps
{
    public class WorkflowBuilder
    {
        private WorkflowBuilder()
        {
        }

        public static WorkflowBuilder CreateBuilder()
        {
            return new WorkflowBuilder();
        }
    }
}