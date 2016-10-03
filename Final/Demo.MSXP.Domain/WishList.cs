using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Demo.MSXP.Domain
{
    public class WishList
    {
        public string Name { get; set; }
        public List<WishListItem> Items { get; set; } 
        public int expiration { get; set; }
    }
}
