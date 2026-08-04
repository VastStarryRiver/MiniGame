using System.Collections.Generic;

namespace CloudService
{
    public class ListSavesResponse
    {
        public List<SaveInfo> saves;
        public int total;
        public int count;
        public int start;
    }
}
