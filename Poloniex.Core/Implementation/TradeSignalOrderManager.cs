namespace Poloniex.Core.Implementation
{
    public class TradeSignalOrderManager
    {
        public static void Tick()
        {
            // query = get oldest order uncompleted order (while nonething is in progress)
            // get oldest unprocessed trade order
            // must be after trade signal order started!
            // otherwise ignore first request if it is a sell order    
            // set record in progress

            // create thread ->
            // execute order logic
            // set database record to isCompleted
            // unlock database record (in progress = false
        }
    }
}
