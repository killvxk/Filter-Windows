using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citadel.IPC.Messages;
using Citadel.Core.Windows.Util;
using CitadelService.Data.Filtering;
using Newtonsoft.Json;
using System.Net;
using Citadel.Platform.Common.Util;

namespace CitadelService.Util
{
    public class Accountability
    {
        private IAppLogger m_logger;

        public Accountability()
        {
            m_logger = LoggerUtil.GetAppWideLogger();

            ReportList = new List<BlockInfo>();
        }

        public List<BlockInfo> ReportList { get; set; }

        public void AddBlockAction(BlockType cause, Uri requestUri, string categoryNameString, string matchingRule)
        {
            LoggerUtil.GetAppWideLogger().Info($"Sending block action to server: cause={cause}, requestUri={requestUri}, categoryNameString={categoryNameString}, matchingRule={matchingRule}");
            BlockInfo blockInfo = new BlockInfo(cause, requestUri, categoryNameString, matchingRule);

            //ReportList.Add(blockInfo);

            string json = JsonConvert.SerializeObject(blockInfo);
            byte[] formData = Encoding.UTF8.GetBytes(json);

            HttpStatusCode statusCode;

            try
            {
                WebServiceUtil.Default.SendResource(ServiceResource.AccountabilityNotify, formData, out statusCode);
            }
            catch(Exception ex)
            {
                LoggerUtil.RecursivelyLogException(m_logger, ex);
            }
        }
    }
}
