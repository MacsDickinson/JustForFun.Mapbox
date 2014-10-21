using Nancy;

namespace JustForFun.Mapbox.Home
{
    using System.Linq;
    using JustForFun.Mapbox.Domain;
    using Raven.Client;

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

            Post["/Test/{title}"] = _ =>
            {
                var title = _.title;

                documentSession.Store(new Location { Title = title });
                documentSession.SaveChanges();

                return title;
            };
        }
    }
}