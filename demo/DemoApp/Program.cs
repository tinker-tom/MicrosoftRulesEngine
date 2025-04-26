// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace DemoApp
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            new BasicDemo().Run();
            Console.WriteLine();
            new JSONDemo().Run();
            Console.WriteLine();
            new NestedInputDemo().Run();
            Console.WriteLine();
            new EFDemo().Run();

            Console.ReadLine();
        }
    }
}