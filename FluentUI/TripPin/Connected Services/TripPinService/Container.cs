﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Generation date: 2024-11-15 12:39:28
namespace FluentUI.TripPin.Model
{
    /// <summary>
    /// There are no comments for Container in the schema.
    /// </summary>
    [global::Microsoft.OData.Client.OriginalNameAttribute("Container")]
    public partial class Container : global::Microsoft.OData.Client.DataServiceContext
    {
        /// <summary>
        /// Initialize a new Container object.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        public Container(global::System.Uri serviceRoot) :
                this(serviceRoot, global::Microsoft.OData.Client.ODataProtocolVersion.V4)
        {
        }

        /// <summary>
        /// Initialize a new Container object.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        public Container(global::System.Uri serviceRoot, global::Microsoft.OData.Client.ODataProtocolVersion protocolVersion) :
                base(serviceRoot, protocolVersion)
        {
            this.ResolveName = new global::System.Func<global::System.Type, string>(this.ResolveNameFromType);
            this.ResolveType = new global::System.Func<string, global::System.Type>(this.ResolveTypeFromName);
            this.OnContextCreated();
            this.Format.LoadServiceModel = GeneratedEdmModel.GetInstance;
            this.Format.UseJson();
        }
        partial void OnContextCreated();
        /// <summary>
        /// Since the namespace configured for this service reference
        /// in Visual Studio is different from the one indicated in the
        /// server schema, use type-mappers to map between the two.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        protected global::System.Type ResolveTypeFromName(string typeName)
        {
            global::System.Type resolvedType = this.DefaultResolveType(typeName, "Trippin", "FluentUI.TripPin.Model");
            if ((resolvedType != null))
            {
                return resolvedType;
            }
            return null;
        }
        /// <summary>
        /// Since the namespace configured for this service reference
        /// in Visual Studio is different from the one indicated in the
        /// server schema, use type-mappers to map between the two.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        protected string ResolveNameFromType(global::System.Type clientType)
        {
            global::Microsoft.OData.Client.OriginalNameAttribute originalNameAttribute = (global::Microsoft.OData.Client.OriginalNameAttribute)global::System.Linq.Enumerable.SingleOrDefault(global::Microsoft.OData.Client.Utility.GetCustomAttributes(clientType, typeof(global::Microsoft.OData.Client.OriginalNameAttribute), true));
            if (clientType.Namespace.Equals("FluentUI.TripPin.Model", global::System.StringComparison.Ordinal))
            {
                if (originalNameAttribute != null)
                {
                    return string.Concat("Trippin.", originalNameAttribute.OriginalName);
                }
                return string.Concat("Trippin.", clientType.Name);
            }
            if (originalNameAttribute != null)
            {
                return clientType.Namespace + "." + originalNameAttribute.OriginalName;
            }
            return clientType.FullName;
        }
        /// <summary>
        /// There are no comments for People in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("People")]
        public virtual global::Microsoft.OData.Client.DataServiceQuery<Person> People
        {
            get
            {
                if ((this._People == null))
                {
                    this._People = base.CreateQuery<Person>("People");
                }
                return this._People;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        private global::Microsoft.OData.Client.DataServiceQuery<Person> _People;
        /// <summary>
        /// There are no comments for Airlines in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("Airlines")]
        public virtual global::Microsoft.OData.Client.DataServiceQuery<Airline> Airlines
        {
            get
            {
                if ((this._Airlines == null))
                {
                    this._Airlines = base.CreateQuery<Airline>("Airlines");
                }
                return this._Airlines;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        private global::Microsoft.OData.Client.DataServiceQuery<Airline> _Airlines;
        /// <summary>
        /// There are no comments for Airports in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("Airports")]
        public virtual global::Microsoft.OData.Client.DataServiceQuery<Airport> Airports
        {
            get
            {
                if ((this._Airports == null))
                {
                    this._Airports = base.CreateQuery<Airport>("Airports");
                }
                return this._Airports;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        private global::Microsoft.OData.Client.DataServiceQuery<Airport> _Airports;
        /// <summary>
        /// There are no comments for People in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        public virtual void AddToPeople(Person person)
        {
            base.AddObject("People", person);
        }
        /// <summary>
        /// There are no comments for Airlines in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        public virtual void AddToAirlines(Airline airline)
        {
            base.AddObject("Airlines", airline);
        }
        /// <summary>
        /// There are no comments for Airports in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        public virtual void AddToAirports(Airport airport)
        {
            base.AddObject("Airports", airport);
        }
        /// <summary>
        /// There are no comments for Me in the schema.
        /// </summary>
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        [global::Microsoft.OData.Client.OriginalNameAttribute("Me")]
        public virtual PersonSingle Me
        {
            get
            {
                if ((this._Me == null))
                {
                    this._Me = new PersonSingle(this, "Me");
                }
                return this._Me;
            }
        }
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        private PersonSingle _Me;
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
        private abstract class GeneratedEdmModel
        {
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
            private static global::Microsoft.OData.Edm.IEdmModel ParsedModel = LoadModelFromString();

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
            private const string filePath = @"TripPinServiceCsdl.xml";

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
            public static global::Microsoft.OData.Edm.IEdmModel GetInstance()
            {
                return ParsedModel;
            }
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
            private static global::Microsoft.OData.Edm.IEdmModel LoadModelFromString()
            {
                global::System.Xml.XmlReader reader = CreateXmlReader();
                try
                {
                    global::System.Collections.Generic.IEnumerable<global::Microsoft.OData.Edm.Validation.EdmError> errors;
                    global::Microsoft.OData.Edm.IEdmModel edmModel;

                    if (!global::Microsoft.OData.Edm.Csdl.CsdlReader.TryParse(reader, true, out edmModel, out errors))
                    {
                        global::System.Text.StringBuilder errorMessages = new global::System.Text.StringBuilder();
                        foreach (var error in errors)
                        {
                            errorMessages.Append(error.ErrorMessage);
                            errorMessages.Append("; ");
                        }
                        throw new global::System.InvalidOperationException(errorMessages.ToString());
                    }

                    return edmModel;
                }
                finally
                {
                    ((global::System.IDisposable)(reader)).Dispose();
                }
            }
            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
            private static global::System.Xml.XmlReader CreateXmlReader(string edmxToParse)
            {
                return global::System.Xml.XmlReader.Create(new global::System.IO.StringReader(edmxToParse));
            }

            [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.OData.Client.Design.T4", "#VersionNumber#")]
            private static global::System.Xml.XmlReader CreateXmlReader()
            {
                try
                {
                    var assembly = global::System.Reflection.Assembly.GetExecutingAssembly();
                    // If multiple resource names end with the file name, select the shortest one.
                    var resourcePath = global::System.Linq.Enumerable.First(
                        global::System.Linq.Enumerable.OrderBy(
                            global::System.Linq.Enumerable.Where(assembly.GetManifestResourceNames(), name => name.EndsWith(filePath)),
                            filteredName => filteredName.Length));
                    global::System.IO.Stream stream = assembly.GetManifestResourceStream(resourcePath);
                    return global::System.Xml.XmlReader.Create(new global::System.IO.StreamReader(stream));
                }
                catch(global::System.Xml.XmlException e)
                {
                    throw new global::System.Xml.XmlException("Failed to create an XmlReader from the stream. Check if the resource exists.", e);
                }
            }
        }
        /// <summary>
        /// There are no comments for GetPersonWithMostFriends in the schema.
        /// </summary>
        [global::Microsoft.OData.Client.OriginalNameAttribute("GetPersonWithMostFriends")]
        public virtual global::FluentUI.TripPin.Model.PersonSingle GetPersonWithMostFriends()
        {
            return new global::FluentUI.TripPin.Model.PersonSingle(this.CreateFunctionQuerySingle<global::FluentUI.TripPin.Model.Person>("", "GetPersonWithMostFriends", false));
        }
        /// <summary>
        /// There are no comments for GetNearestAirport in the schema.
        /// </summary>
        [global::Microsoft.OData.Client.OriginalNameAttribute("GetNearestAirport")]
        public virtual global::FluentUI.TripPin.Model.AirportSingle GetNearestAirport(double lat, double lon)
        {
            return new global::FluentUI.TripPin.Model.AirportSingle(this.CreateFunctionQuerySingle<global::FluentUI.TripPin.Model.Airport>("", "GetNearestAirport", false, new global::Microsoft.OData.Client.UriOperationParameter("lat", lat),
                    new global::Microsoft.OData.Client.UriOperationParameter("lon", lon)));
        }
        /// <summary>
        /// There are no comments for ResetDataSource in the schema.
        /// </summary>
        [global::Microsoft.OData.Client.OriginalNameAttribute("ResetDataSource")]
        public virtual global::Microsoft.OData.Client.DataServiceActionQuery ResetDataSource()
        {
            return new global::Microsoft.OData.Client.DataServiceActionQuery(this, this.BaseUri.OriginalString.Trim('/') + "/ResetDataSource");
        }
    }
}
