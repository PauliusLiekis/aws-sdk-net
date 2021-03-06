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
 * Do not modify this file. This file is generated from the route53-2013-04-01.normal.json service model.
 */
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Text;
using System.IO;

using Amazon.Runtime;
using Amazon.Runtime.Internal;

namespace Amazon.Route53.Model
{
    /// <summary>
    /// Container for the parameters to the UpdateHostedZoneComment operation.
    /// To update the hosted zone comment, send a <code>POST</code> request to the <code>/<i>Route
    /// 53 API version</i>/hostedzone/<i>hosted zone ID</i></code> resource. The request body
    /// must include a document with a <code>UpdateHostedZoneCommentRequest</code> element.
    /// The response to this request includes the modified <code>HostedZone</code> element.
    /// 
    ///  <note> The comment can have a maximum length of 256 characters.</note>
    /// </summary>
    public partial class UpdateHostedZoneCommentRequest : AmazonRoute53Request
    {
        private string _id;
        private string _comment;

        /// <summary>
        /// Gets and sets the property Id. 
        /// <para>
        /// The ID of the hosted zone you want to update.
        /// </para>
        /// </summary>
        public string Id
        {
            get { return this._id; }
            set { this._id = value; }
        }

        // Check to see if Id property is set
        internal bool IsSetId()
        {
            return this._id != null;
        }

        /// <summary>
        /// Gets and sets the property Comment. 
        /// <para>
        /// A comment about your hosted zone.
        /// </para>
        /// </summary>
        public string Comment
        {
            get { return this._comment; }
            set { this._comment = value; }
        }

        // Check to see if Comment property is set
        internal bool IsSetComment()
        {
            return this._comment != null;
        }

    }
}