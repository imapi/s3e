﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

using com.amazon.s3;

using s3.Options;

namespace s3.Commands
{
    class Delete : Command
    {
        string bucket, prefix;
        Regex regex = null;

        protected override void Initialise(CommandLine cl)
        {
            if (cl.args.Count == 1)
            {
                int slashIdx = cl.args[0].IndexOf("/");
                if (slashIdx == -1)
                {
                    bucket = cl.args[0];
                    prefix = "";
                }
                else
                {
                    bucket = cl.args[0].Substring(0, slashIdx);
                    prefix = cl.args[0].Substring(slashIdx + 1);
                }
            }
            else
                throw new SyntaxException("The delete command requires either zero or one parameters");

            if (cl.options.ContainsKey(typeof(s3.Options.Rex)))
            {
                regex = new Regex((cl.options[typeof(s3.Options.Rex)] as s3.Options.Rex).Parameter, RegexOptions.Compiled);
            }
        }

        public override void Execute()
        {
            AWSAuthConnection svc = new AWSAuthConnection();

            if (prefix.EndsWith("*"))
                prefix = prefix.Substring(0, prefix.Length - 1);

            int fileCount = 0;
            long fileSize = 0;
            foreach (ListEntry e in new IterativeList(bucket, prefix, regex))
            {
                Response response = svc.delete(bucket, e.Key, null);
                Console.WriteLine(string.Format("{0}\t{1,14:##,#}\t{2} {3}", e.LastModified, e.Size, e.Key, response.Status == System.Net.HttpStatusCode.NoContent ? "-" : response.Status.ToString()));
                fileCount++;
                fileSize += e.Size;
            }

            Console.Error.WriteLine(string.Format("{0} files, {1:##,#} bytes", fileCount, fileSize));
        }

    }
}

