/*
 * Copyright 2010-2014 Amazon.com, Inc. or its affiliates. All Rights Reserved.
 * 
 * Licensed under the Apache License, Version 2.0 (the "License").
 * You may not use this file except in compliance with the License.
 * A copy of the License is located at
 * 
 *  http://aws.amazon.com/apache2.0
 * 
 * or in the "license" file accompanying this file. This file is distributed
 * on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either
 * express or implied. See the License for the specific language governing
 * permissions and limitations under the License.
 */

/*
 * Do not modify this file. This file is generated from the monitoring-2010-08-01.normal.json service model.
 */
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Serialization;

using Amazon.CloudWatch.Model;
using Amazon.Runtime;
using Amazon.Runtime.Internal;
using Amazon.Runtime.Internal.Transform;
using Amazon.Runtime.Internal.Util;
namespace Amazon.CloudWatch.Model.Internal.MarshallTransformations
{
    /// <summary>
    /// Response Unmarshaller for AlarmHistoryItem Object
    /// </summary>  
    public class AlarmHistoryItemUnmarshaller : IUnmarshaller<AlarmHistoryItem, XmlUnmarshallerContext>, IUnmarshaller<AlarmHistoryItem, JsonUnmarshallerContext>
    {
        public AlarmHistoryItem Unmarshall(XmlUnmarshallerContext context)
        {
            AlarmHistoryItem unmarshalledObject = new AlarmHistoryItem();
            int originalDepth = context.CurrentDepth;
            int targetDepth = originalDepth + 1;
            
            if (context.IsStartOfDocument) 
               targetDepth += 2;
            
            while (context.ReadAtDepth(originalDepth))
            {
                if (context.IsStartElement || context.IsAttribute)
                {
                    if (context.TestExpression("AlarmName", targetDepth))
                    {
                        var unmarshaller = StringUnmarshaller.Instance;
                        unmarshalledObject.AlarmName = unmarshaller.Unmarshall(context);
                        continue;
                    }
                    if (context.TestExpression("HistoryData", targetDepth))
                    {
                        var unmarshaller = StringUnmarshaller.Instance;
                        unmarshalledObject.HistoryData = unmarshaller.Unmarshall(context);
                        continue;
                    }
                    if (context.TestExpression("HistoryItemType", targetDepth))
                    {
                        var unmarshaller = StringUnmarshaller.Instance;
                        unmarshalledObject.HistoryItemType = unmarshaller.Unmarshall(context);
                        continue;
                    }
                    if (context.TestExpression("HistorySummary", targetDepth))
                    {
                        var unmarshaller = StringUnmarshaller.Instance;
                        unmarshalledObject.HistorySummary = unmarshaller.Unmarshall(context);
                        continue;
                    }
                    if (context.TestExpression("Timestamp", targetDepth))
                    {
                        var unmarshaller = DateTimeUnmarshaller.Instance;
                        unmarshalledObject.Timestamp = unmarshaller.Unmarshall(context);
                        continue;
                    }
                }
                else if (context.IsEndElement && context.CurrentDepth < originalDepth)
                {
                    return unmarshalledObject;
                }
            }

            return unmarshalledObject;
        }

        public AlarmHistoryItem Unmarshall(JsonUnmarshallerContext context)
        {
            return null;
        }


        private static AlarmHistoryItemUnmarshaller _instance = new AlarmHistoryItemUnmarshaller();        

        public static AlarmHistoryItemUnmarshaller Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}