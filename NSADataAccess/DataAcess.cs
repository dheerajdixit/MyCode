using System;
using System.Collections.Generic;
using System.Text;
using MongoDB;

namespace NSADataAccess
{
    class DataAcess
    {

        public void GetData()
        {
            MongoDataAccess.GetHistory("", 0, DateTime.Now, DateTime.Now);
        }
    }
}
