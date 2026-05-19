namespace Skyline.DataMiner.Solutions.Relationships.Tests
{
	using System;
	using System.Collections.Generic;
	using System.Data.Common;
	using System.Linq;
	using System.Text;
	using System.Threading.Tasks;

	using Moq;

	using Skyline.DataMiner.Net;
	using Skyline.DataMiner.Net.Apps.DataMinerObjectModel;
	using Skyline.DataMiner.Net.ManagerStore;
	using Skyline.DataMiner.Net.Messages;
	using Skyline.DataMiner.Solutions.Relationships.Install.DOM;
	using Skyline.DataMiner.Utils.DOM.UnitTesting;

	internal class Mock
	{
		public static IConnection CreateConnection()
		{
			var connection = new DomConnectionMock();
			Func<DMSMessage[], DMSMessage[]> handleMessage = (DMSMessage[] msgs) =>
			{
				var result = new List<DMSMessage>();
				foreach (var msg in msgs)
				{
					if (msg is ManagerStoreReinitializeCustomManagerRequest reinitMsg)
					{
						result.Add((DMSMessage)reinitMsg.CreateFromTraceData(new TraceData()));
						continue;
					}

					result.AddRange(connection.HandleMessage(msg));
				}

				return result.ToArray();
			};

			var temp = new Mock<IConnection>();
			temp.Setup(c => c.HandleMessage(It.IsAny<DMSMessage>())).Returns((DMSMessage msg) => handleMessage([msg]));
			temp.Setup(c => c.HandleMessages(It.IsAny<DMSMessage[]>())).Returns((DMSMessage[] msgs) => handleMessage(msgs));
			temp.Setup(c => c.HandleSingleResponseMessage(It.IsAny<DMSMessage>())).Returns((DMSMessage msg) => handleMessage([msg]).First());
			new DomInstaller(temp.Object).InstallDefaultContent();

			// Remove all instances of the default content, as this is not needed for the tests and may interfere with them.
			var helper = new DomHelper(temp.Object.HandleMessages, "(sdm)relationships");
			var instances = helper.DomInstances.ReadAll();
			helper.DomInstances.Delete(instances);

			return temp.Object;
		}
	}
}
