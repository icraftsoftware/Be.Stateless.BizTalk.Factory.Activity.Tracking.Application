namespace Be.Stateless.BizTalk.Schemas.Xml {
    using Microsoft.XLANGs.BaseTypes;
    
    
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.BizTalk.Schema.Compiler", "3.0.1.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [SchemaType(SchemaTypeEnum.Document)]
    [System.SerializableAttribute()]
    [SchemaRoots(new string[] {@"Check", @"CheckIn", @"CheckOut"})]
    public sealed class Claim : Microsoft.XLANGs.BaseTypes.SchemaBase {
        
        [System.NonSerializedAttribute()]
        private static object _rawSchema;
        
        [System.NonSerializedAttribute()]
        private const string _strSchema = @"<?xml version=""1.0"" encoding=""utf-16""?>
<xs:schema xmlns=""urn:schemas.stateless.be:biztalk:claim:2017:04"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" elementFormDefault=""qualified"" targetNamespace=""urn:schemas.stateless.be:biztalk:claim:2017:04"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:annotation>
    <xs:documentation><![CDATA[
Copyright © 2012 - 2020 François Chabot

Licensed under the Apache License, Version 2.0 (the ""License"");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an ""AS IS"" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
]]></xs:documentation>
  </xs:annotation>
  <xs:element name=""Check"">
    <xs:annotation>
      <xs:appinfo>
        <b:recordInfo notes=""Token message that replaces a business message body payload's stream when it is checked in a local claim store, i.e. not a network-shared file system. When checked in a local store, the payload does not need to make its way to a central store; it is already in the store. Consequently, the claim token does not need to be approved for checkout by the ClaimStore.Agent and is readily available for &quot;checkout.&quot;"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" />
        <san:Properties xmlns:bf=""urn:schemas.stateless.be:biztalk:properties:system:2012:04"" xmlns:cp=""urn:schemas.stateless.be:biztalk:properties:claim:2017:04"" xmlns:tp=""urn:schemas.stateless.be:biztalk:properties:tracking:2012:04"" xmlns:san=""urn:schemas.stateless.be:biztalk:annotations:2013:01"">
          <cp:MessageType mode=""promote"" xpath=""/*/*[local-name()='MessageType']"" />
          <bf:CorrelationId mode=""promote"" xpath=""/*/*[local-name()='CorrelationId']"" />
          <bf:EnvironmentTag mode=""promote"" xpath=""/*/*[local-name()='EnvironmentTag']"" />
          <bf:OutboundTransportLocation xpath=""/*/*[local-name()='OutboundTransportLocation']"" />
          <tp:ProcessActivityId xpath=""/*/*[local-name()='ProcessActivityId']"" />
          <bf:ReceiverName mode=""promote"" xpath=""/*/*[local-name()='ReceiverName']"" />
          <bf:SenderName mode=""promote"" xpath=""/*/*[local-name()='SenderName']"" />
          <tp:Value1 xpath=""/*/*[local-name()='CorrelationId']"" />
          <tp:Value2 xpath=""/*/*[local-name()='ReceiverName']"" />
          <tp:Value3 xpath=""/*/*[local-name()='SenderName']"" />
        </san:Properties>
      </xs:appinfo>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:group ref=""TokenType"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""CheckIn"">
    <xs:annotation>
      <xs:appinfo>
        <b:recordInfo notes=""Inbound token message that replaces a business message body payload's stream when it is checked in a central claim store, i.e. a network-shared file system. Notice that a message is always checked in locally and moved asynchronously to a central claim store by the ClaimStore.Agent. That is why, later on, the CheckIn token will be transformed into a CheckOut token when the actual business message body payload has been brought to the central claim store."" />
        <san:Properties xmlns:tp=""urn:schemas.stateless.be:biztalk:properties:tracking:2012:04"" xmlns:san=""urn:schemas.stateless.be:biztalk:annotations:2013:01"">
          <tp:Value1 xpath=""/*/*[local-name()='CorrelationId']"" />
          <tp:Value2 xpath=""/*/*[local-name()='ReceiverName']"" />
          <tp:Value3 xpath=""/*/*[local-name()='SenderName']"" />
        </san:Properties>
      </xs:appinfo>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:group ref=""TokenType"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:element name=""CheckOut"">
    <xs:annotation>
      <xs:appinfo>
        <b:recordInfo notes=""Outbound token message that has to be replaced by the original business message body payload's stream, which has been priorly checked in the central claim store. Notice that a message is always checked in locally and moved asynchronously to a central claim store. That is why, the initial CheckIn token has to be transformed into a CheckOut token when the ClaimStore.Agent has made available the actual business message body payload in the central claim store."" />
        <san:Properties xmlns:bf=""urn:schemas.stateless.be:biztalk:properties:system:2012:04"" xmlns:cp=""urn:schemas.stateless.be:biztalk:properties:claim:2017:04"" xmlns:tp=""urn:schemas.stateless.be:biztalk:properties:tracking:2012:04"" xmlns:san=""urn:schemas.stateless.be:biztalk:annotations:2013:01"">
          <cp:MessageType mode=""promote"" xpath=""/*/*[local-name()='MessageType']"" />
          <bf:CorrelationId mode=""promote"" xpath=""/*/*[local-name()='CorrelationId']"" />
          <bf:EnvironmentTag mode=""promote"" xpath=""/*/*[local-name()='EnvironmentTag']"" />
          <bf:OutboundTransportLocation xpath=""/*/*[local-name()='OutboundTransportLocation']"" />
          <tp:ProcessActivityId xpath=""/*/*[local-name()='ProcessActivityId']"" />
          <bf:ReceiverName mode=""promote"" xpath=""/*/*[local-name()='ReceiverName']"" />
          <bf:SenderName mode=""promote"" xpath=""/*/*[local-name()='SenderName']"" />
          <tp:Value1 xpath=""/*/*[local-name()='CorrelationId']"" />
          <tp:Value2 xpath=""/*/*[local-name()='ReceiverName']"" />
          <tp:Value3 xpath=""/*/*[local-name()='SenderName']"" />
        </san:Properties>
      </xs:appinfo>
    </xs:annotation>
    <xs:complexType>
      <xs:sequence>
        <xs:group ref=""TokenType"" />
      </xs:sequence>
    </xs:complexType>
  </xs:element>
  <xs:group name=""TokenType"">
    <xs:sequence>
      <xs:element minOccurs=""0"" name=""CorrelationId"" type=""xs:string"" />
      <xs:element minOccurs=""0"" name=""EnvironmentTag"" type=""xs:string"" />
      <xs:element minOccurs=""0"" name=""MessageType"" type=""xs:string"" />
      <xs:element minOccurs=""0"" name=""OutboundTransportLocation"" type=""xs:string"" />
      <xs:element minOccurs=""0"" name=""ProcessActivityId"" type=""xs:string"" />
      <xs:element minOccurs=""0"" name=""ReceiverName"" type=""xs:string"" />
      <xs:element minOccurs=""0"" name=""SenderName"" type=""xs:string"" />
      <xs:element name=""Url"" type=""xs:string"" />
      <xs:any minOccurs=""0"" maxOccurs=""unbounded"" processContents=""lax"" />
    </xs:sequence>
  </xs:group>
</xs:schema>";
        
        public Claim() {
        }
        
        public override string XmlContent {
            get {
                return _strSchema;
            }
        }
        
        public override string[] RootNodes {
            get {
                string[] _RootElements = new string [3];
                _RootElements[0] = "Check";
                _RootElements[1] = "CheckIn";
                _RootElements[2] = "CheckOut";
                return _RootElements;
            }
        }
        
        protected override object RawSchema {
            get {
                return _rawSchema;
            }
            set {
                _rawSchema = value;
            }
        }
        
        [Schema(@"urn:schemas.stateless.be:biztalk:claim:2017:04",@"Check")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"Check"})]
        public sealed class Check : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public Check() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "Check";
                    return _RootElements;
                }
            }
            
            protected override object RawSchema {
                get {
                    return _rawSchema;
                }
                set {
                    _rawSchema = value;
                }
            }
        }
        
        [Schema(@"urn:schemas.stateless.be:biztalk:claim:2017:04",@"CheckIn")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"CheckIn"})]
        public sealed class CheckIn : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public CheckIn() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "CheckIn";
                    return _RootElements;
                }
            }
            
            protected override object RawSchema {
                get {
                    return _rawSchema;
                }
                set {
                    _rawSchema = value;
                }
            }
        }
        
        [Schema(@"urn:schemas.stateless.be:biztalk:claim:2017:04",@"CheckOut")]
        [System.SerializableAttribute()]
        [SchemaRoots(new string[] {@"CheckOut"})]
        public sealed class CheckOut : Microsoft.XLANGs.BaseTypes.SchemaBase {
            
            [System.NonSerializedAttribute()]
            private static object _rawSchema;
            
            public CheckOut() {
            }
            
            public override string XmlContent {
                get {
                    return _strSchema;
                }
            }
            
            public override string[] RootNodes {
                get {
                    string[] _RootElements = new string [1];
                    _RootElements[0] = "CheckOut";
                    return _RootElements;
                }
            }
            
            protected override object RawSchema {
                get {
                    return _rawSchema;
                }
                set {
                    _rawSchema = value;
                }
            }
        }
    }
}
