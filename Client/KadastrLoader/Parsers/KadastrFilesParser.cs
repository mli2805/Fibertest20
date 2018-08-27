using System.Threading.Tasks;

namespace KadastrLoader
{
    public class KadastrFilesParser
    {
        private readonly WellParser _wellParser;
        private readonly ChannelParser _channelParser;
        private readonly ConpointParser _conpointParser;

        public KadastrFilesParser(WellParser wellParser, ChannelParser channelParser, ConpointParser conpointParser)
        {
            _wellParser = wellParser;
            _channelParser = channelParser;
            _conpointParser = conpointParser;
        }

        public async Task<int> Go(string folder)
        {
            await _wellParser.ParseWells(folder);
            await _channelParser.ParseChannels(folder);
            await _conpointParser.ParseConpoints(folder);
            return 1;
        }
    }
}