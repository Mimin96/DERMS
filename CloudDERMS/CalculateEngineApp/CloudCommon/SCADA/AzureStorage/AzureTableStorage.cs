using CloudCommon.SCADA.AzureStorage.Entities;
using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudCommon.SCADA.AzureStorage
{
    public static class AzureTableStorage
    {
        /*
         Izmene treba napraviti u klasama:
                MainViewModel               -> dCom.ViewModel
                AnalogBase                  -> dCom.ViewModel
                HistoryUserControlViewModel -> UI.ViewModel
        */

        //connectionString: "UseDevelopmentStorage=true;"
        public static bool AddTableEntityInDB<T>(T entity, string connectionString, string tableName) where T : TableEntity
        {
            try
            {
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Retrieve a reference to the table.
                CloudTable table = tableClient.GetTableReference(tableName);

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                //Add Entity into table
                TableOperation insertOperation = TableOperation.InsertOrReplace(entity);

                table.Execute(insertOperation);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool InsertEntitiesInDB<T>(List<T> entities, string connectionString, string tableName) where T : TableEntity
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "people" table.
                CloudTable table = tableClient.GetTableReference(tableName);

                // Create the table if it doesn't exist.
                table.CreateIfNotExists();

                // Create the batch operation.
                TableBatchOperation batchOperation = new TableBatchOperation();

                entities.ForEach(x => {
                    batchOperation.InsertOrReplace(x);
                });

                // Execute the batch operation.
                table.ExecuteBatch(batchOperation);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public static DynamicTableEntity GetSingleEntityFromDB<T>(string partitionKey, string rowKey, string connectionString, string tableName) where T : TableEntity
        {
            try
            {
                // Retrieve the storage account from the connection string.
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

                // Create the table client.
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                // Create the CloudTable object that represents the "people" table.
                CloudTable table = tableClient.GetTableReference(tableName);

                // Create a retrieve operation that takes a customer entity.
                TableOperation retrieveOperation = TableOperation.Retrieve(partitionKey, rowKey);

                // Execute the retrieve operation.
                TableResult retrievedResult = table.Execute(retrieveOperation);

                if (retrievedResult != null)
                {
                    // Print the phone number of the result.
                    return (DynamicTableEntity)retrievedResult.Result;
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public static List<CollectItem> GetAllCollectItems(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<CollectItem> entities = new List<CollectItem>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<CollectItem>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }
        public static List<DayItem> GetAllDayItems(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<DayItem> entities = new List<DayItem>();
           
            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<DayItem>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }
        public static List<MonthItem> GetAllMonthItems(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<MonthItem> entities = new List<MonthItem>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<MonthItem>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }
        public static List<YearItem> GetAllYearItems(string connectionString, string tableName)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create the table client.
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            // Create the CloudTable object that represents the "people" table.
            CloudTable table = tableClient.GetTableReference(tableName);

            TableContinuationToken token = null;
            List<YearItem> entities = new List<YearItem>();

            do
            {
                var queryResult = table.ExecuteQuerySegmented(new TableQuery<YearItem>(), token);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return entities;
        }

        //public static bool DeleteEntityInDB(string partitionKey, string rowKey, string connectionString, string tableName)
        //{
        //    // Retrieve the storage account from the connection string.
        //    CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);

        //    // Create the table client.
        //    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

        //    // Create the CloudTable that represents the "people" table.
        //    CloudTable table = tableClient.GetTableReference(tableName);

        //    // Create a retrieve operation that expects a customer entity.
        //    TableOperation retrieveOperation = TableOperation.Retrieve<ITableEntity>(partitionKey, rowKey);

        //    // Execute the operation.
        //    TableResult retrievedResult = table.Execute(retrieveOperation);

        //    // Assign the result to a CustomerEntity.
        //    UserEntity deleteEntity = (UserEntity)retrievedResult.Result;

        //    // Create the Delete TableOperation.
        //    if (deleteEntity != null)
        //    {
        //        TableOperation deleteOperation = TableOperation.Delete(deleteEntity);
        //        table.Execute(deleteOperation);

        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}
