using System.Data;
using FluentMigrator;
using NzbDrone.Core.Datastore.Migration.Framework;

namespace NzbDrone.Core.Datastore.Migration
{
    [Migration(132)]
    public class add_download_client_priority : NzbDroneMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("DownloadClients").AddColumn("Priority").AsInt32().WithDefaultValue(1);
            Execute.WithConnection(InitPriorityForBackwardCompatibility);

        }

        private void InitPriorityForBackwardCompatibility(IDbConnection conn, IDbTransaction tran)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Transaction = tran;
                cmd.CommandText = "SELECT Id FROM DownloadClients WHERE Enable = 1";

                using (var reader = cmd.ExecuteReader())
                {
                    int i = 1;
                    while (reader.Read())
                    {
                        var id = reader.GetInt32(0);
                        
                        using (var updateCmd = conn.CreateCommand())
                        {
                            updateCmd.Transaction = tran;
                            updateCmd.CommandText = "UPDATE DownloadClients SET Priority = ? WHERE Id = ?";
                            updateCmd.AddParameter(i++);
                            updateCmd.AddParameter(id);

                            updateCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
    }
}
