// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();

            app.UseMvc();

            // Migrate and seed the database during startup. Must be synchronous.

            //Check if it's a fresh DB. Else insert a record 'Initial' to migration table if it's not already existing.
            try
            {
                using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
                    .CreateScope())
                {
                    var connection = Configuration.GetConnectionString("DefaultConnection");
                    using (SqlConnection conn = new SqlConnection(connection))
                    {
                        SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM SYSOBJECTS WHERE xtype = 'U'", conn);
                        try
                        {
                            conn.Open();
                            int noOfTables = (int)cmd.ExecuteScalar();

                            if (noOfTables == 40)
                            {
                                new SqlCommand("INSERT INTO [dbo].[__EFMigrationsHistory] VALUES ('20191119102307_Initial','2.0.0-rtm-26452')", conn).ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    
                    serviceScope.ServiceProvider.GetService<VotingAppDBContext>().Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
