using Mongoose.IDO.Protocol;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CNHDevelopmentTaskExample
{
    internal class CNH_AssemblyUtils
    {
        public static IDOItem FlattenItem(IDOItem item)
        {
            IDOItem unionedItem = item;
            if(item.NestedResponses.Count == 0)
            {
                return item;
            }
            foreach (LoadCollectionResponseData nestedResponse in item.NestedResponses)
            {
              foreach (IDOItem nestedItem in nestedResponse.Items)
              {
                  var flattenedNestedItem = FlattenItem(nestedItem);
                  unionedItem.PropertyValues.AddRange(flattenedNestedItem.PropertyValues);
              }
            }
            return unionedItem;
        }
    }
}
