using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BotFrameworkSample.Bots;
using BotFrameworkSample.Dialogs;
using BotFrameworkSample.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotFrameworkSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment hostingEnvironment)
        {
            Configuration = configuration;
            HostingEnvironment = hostingEnvironment;
        }

        public IConfiguration Configuration { get; set; }

        public IHostingEnvironment HostingEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //Configurar el adapter
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            //Configurar estados de bos
            ConfigureState(services);

            //Configurar los dialogos
            ConfigureDialogs(services);

            services.AddTransient<IBot, DialogBot<MainDialog>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles();

            app.UseStaticFiles();

            app.UseMvc();
        }

        private void ConfigureState(IServiceCollection services)
        {

            if (!HostingEnvironment.IsDevelopment())
            {
                services.AddSingleton<IStorage, MemoryStorage>();
            }
            else
            {
                string connString = Configuration["ConnectionStringStorageBot"];
                string containerName = Configuration["ContainerNameBot"];
                services.AddSingleton<IStorage, AzureBlobStorage>(provider =>
                    new AzureBlobStorage(connString, containerName));
            }

            services.AddSingleton<UserState>();

            services.AddSingleton<ConversationState>();

            services.AddSingleton<BotStateService>();
        }

        private void ConfigureDialogs(IServiceCollection service)
        {
            int minutosVencerNumero = int.Parse(Configuration["minutosVencerNumero"]);
            service.AddSingleton<Dialog, MainDialog>(x => new MainDialog(x.GetService<BotStateService>(), minutosVencerNumero));
        }

    }
}
