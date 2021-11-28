using System;
using System.Collections.Generic;
using System.Text;

namespace HMTSolution.Infrastructure.Settings
{
    public class MongoSettings
    {
        public string? ConnectionString;
        public string? Database;

        #region Configuration Values

        public const string ConnectionStringValue = nameof(ConnectionString);
        public const string DatabaseValue = nameof(Database);

        #endregion
    }
}
