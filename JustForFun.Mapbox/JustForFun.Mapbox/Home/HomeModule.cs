using Nancy;

namespace JustForFun.Mapbox.Home
{
    using System.Collections.Generic;
    using System.Linq;
    using GeoJSON.Net.Feature;
    using JustForFun.Mapbox.Domain;
    using Nancy.ModelBinding;
    using Nancy.Responses;
    using Raven.Client;
    using Raven.Imports.Newtonsoft.Json;
    using Raven.Imports.Newtonsoft.Json.Serialization;

    public class HomeModule : NancyModule
    {
        public HomeModule(IDocumentSession documentSession)
        {
            Get["/"] = _ =>
            {
                var posts = documentSession.Query<Location>().ToList();

                return Negotiate.WithView("Home")
                                .WithModel(posts);
            };

            Get["/Edit"] = _ =>
            {
                var posts = documentSession.Query<Location>().ToList();

                return Negotiate.WithView("Edit")
                                .WithModel(posts);
            };

            Get["/New"] = _ =>
            {
                return Negotiate.WithView("New")
                                .WithModel(new Location());
            };

            Post["/New"] = _ =>
            {
                var model = this.Bind<Location>();

                documentSession.Store(model);

                return new RedirectResponse("/");
            };

            Post["/Test/{title}"] = _ =>
            {
                var title = _.title;

                documentSession.Store(new Location
                {
                    Title = title,
                    Description = "Test",
                    Latitude = 0.4,
                    Longitude = 1.2
                });

                return title;
            };

            Post["/Update"] = _ =>
            {
                var model = this.Bind<Location>();

                var domain = documentSession.Load<Location>(model.Id);
                domain.Title = model.Title;
                domain.Description = model.Description;
                domain.Latitude = model.Latitude;
                domain.Longitude = model.Longitude;


                documentSession.Store(domain);

                return new RedirectResponse("/");
            };

            Get["/MapData"] = _ =>
            {
                var posts = documentSession.Query<Location>().ToList().Select(x =>
                {
                    var point = new GeoJSON.Net.Geometry.Point(new GeoJSON.Net.Geometry.GeographicPosition(x.Latitude, x.Longitude));

                    var featureProperties = new Dictionary<string, object>
                    {
                        { "title", x.Title },
                        { "description", x.Description },
                        { "marker-size", "large" },
                        { "marker-color", "#6FCAC5" },
                        { "marker-symbol", "star" }
                    };
                    return new Feature(point, featureProperties);
                });

                var serializedData = JsonConvert.SerializeObject(posts, Formatting.Indented, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore });

                return serializedData;
            };
        }
    }
}