using System.Collections.Generic;
using YooAsset;

namespace Patch.Test.Scripts
{
    public class SampleRemoteService : IRemoteService
    {
        private readonly string RemoteUrl = "https://cdn.zongyigame.com/wx/SoccerStar/TT/1.0.9/Test/Test/";
        
        public IReadOnlyList<string> GetRemoteUrls(string fileName)
        {
            return new List<string>()
            {
                RemoteUrl + fileName,
            };
        }
    }
}