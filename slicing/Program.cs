using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using slicing.builder;

namespace slicing
{
    class Program
    {
        static void Main(string[] args)
        {
            PDGBuilder pdgBuilder = new PDGBuilder();
            //var filePath = "C:\\File_VA\\c#\\NET\\00a1c7dff517266b7e001dd607952072";
            var filePath = "C:\\File_VA\\copyfolder3.exe";
            pdgBuilder.Build(filePath);

        }
    }
}
