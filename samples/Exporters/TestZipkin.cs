﻿// <copyright file="TestZipkin.cs" company="OpenTelemetry Authors">
// Copyright 2018, OpenTelemetry Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.Threading;
using OpenTelemetry.Trace;
using OpenTelemetry.Trace.Configuration;

namespace Samples
{
    internal class TestZipkin
    {
        internal static object Run(string zipkinUri)
        {
            // Configure exporter to export traces to Zipkin
            using (var tracerFactory = TracerFactory.Create(builder => builder
                .UseZipkin(o =>
                {
                    o.ServiceName = "test-zipkin";
                    o.Endpoint = new Uri(zipkinUri);
                })))
            {
                var tracer = tracerFactory.GetTracer("zipkin-test");

                // Create a scoped span. It will end automatically when using statement ends
                using (tracer.WithSpan(tracer.StartSpan("Main")))
                {
                    Console.WriteLine("About to do a busy work");
                    for (var i = 0; i < 10; i++)
                    {
                        DoWork(i, tracer);
                    }
                }
            }

            return null;
        }

        private static void DoWork(int i, ITracer tracer)
        {
            // Start another span. If another span was already started, it'll use that span as the parent span.
            // In this example, the main method already started a span, so that'll be the parent span, and this will be
            // a child span.
            using (tracer.WithSpan(tracer.StartSpan("DoWork")))
            {
                // Simulate some work.
                var span = tracer.CurrentSpan;

                try
                {
                    Console.WriteLine("Doing busy work");
                    Thread.Sleep(1000);
                }
                catch (ArgumentOutOfRangeException e)
                {
                    // Set status upon error
                    span.Status = Status.Internal.WithDescription(e.ToString());
                }

                // Annotate our span to capture metadata about our operation
                var attributes = new Dictionary<string, object>();
                attributes.Add("use", "demo");
                span.AddEvent("Invoking DoWork", attributes);
            }
        }
    }
}
