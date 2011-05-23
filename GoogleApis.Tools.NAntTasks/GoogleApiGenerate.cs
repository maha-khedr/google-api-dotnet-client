/*
Copyright 2010 Google Inc

Licensed under the Apache License, Version 2.0 (the ""License"");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an ""AS IS"" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;

using NAnt.Core;
using NAnt.Core.Attributes;

using Google.Apis.Discovery;
using Google.Apis.Tools.CodeGen;
using Google.Apis.Util;

namespace Google.Apis.Tools.NAntTasks
{
   [TaskName("googleapigenerate")]
   public class GoogleApiGenerate : Task
    {
        [TaskAttribute("discoveryurl", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string DiscoveryUrl {get; set;}
        
        [TaskAttribute("outputfile", Required = true)]        
        public FileInfo OutputFile {get; set;} 
        
        [TaskAttribute("clientnamespace", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ClientNamespace {get; set;}
        
        [TaskAttribute("apiversion", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string ApiVersion {get; set;}
        
        [TaskAttribute("baseurl", Required = true)]
        [StringValidator(AllowEmpty = false)]
        public string BaseUrl {get; set;}
        
        protected override void ExecuteTask ()
        {
            DiscoveryUrl.ThrowIfNullOrEmpty("DiscoveryUrl");
            OutputFile.ThrowIfNull("OutputFile");
            ClientNamespace.ThrowIfNullOrEmpty("ClientNamespace");
            ApiVersion.ThrowIfNullOrEmpty("ApiVersion");
            BaseUrl.ThrowIfNullOrEmpty("BaseUrl");
            
            Project.Log(Level.Info, "Fetching Discovery " + DiscoveryUrl);
            var fetcher = new WebDiscoveryDevice(new Uri(DiscoveryUrl));
            var discovery = new DiscoveryService(fetcher);
            var param = new FactoryParameterV1_0(BaseUrl, null);
            var service = discovery.GetService(ApiVersion, DiscoveryVersion.Version_1_0, param);
            var generator = new GoogleServiceGenerator(service, ClientNamespace);
            var provider = CodeDomProvider.CreateProvider("CSharp");
            Project.Log(Level.Info, "Generating To File " + OutputFile.FullName);
            using (StreamWriter sw = new StreamWriter (OutputFile.FullName, false)) {
                    IndentedTextWriter tw = new IndentedTextWriter (sw, "  ");
                    provider.GenerateCodeFromCompileUnit (
                        generator.GenerateCode (), tw, new CodeGeneratorOptions ());
                    tw.Close ();
            }
        }
    }
}