using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace instabot
{
    class Cycle
    { 
        static int[] getAverageInteractions(IWebDriver driver, int delay, string name)
        {
            // stats[0] is likes
            // stats[1] is comments
            int[] stats = {0,0};

            int postCount = int.Parse(driver.FindElement(By.XPath("//li[1]/span/span")).Text); 
            
            if(postCount>3){postCount=3;}           

            Actions actions = new Actions(driver);

            for(int i=1;i<postCount+1;i++)
            {
                IWebElement x = driver.FindElement(By.XPath("//div["+i+"]/a/div/div[2]"));
                actions.MoveToElement(x).Build().Perform(); 
                try
                {
                    stats[1] = stats[1]+int.Parse(driver.FindElement(By.XPath("//div["+i+"]/a/div[2]/ul/li[2]/span[1]")).Text);
                }
                catch{
                    stats[1] = stats[1]+int.Parse(driver.FindElement(By.XPath("//div["+i+"]/a/div[3]/ul/li[2]/span[1]")).Text);
                }

                try
                {
                    stats[0] = stats[0]+int.Parse(driver.FindElement(By.XPath("//div["+i+"]/a/div[2]/ul/li[1]/span[1]")).Text);
                }
                catch{
                    stats[0] = stats[0]+int.Parse(driver.FindElement(By.XPath("//div["+i+"]/a/div[3]/ul/li[1]/span[1]")).Text);
                }
            }

            Console.WriteLine(stats[0]+"\n"+stats[1]);

            for(int i=0;i<2;i++){stats[i]=stats[i]/postCount;}
            
            return stats;  
        }
        //Clears followers
        static int[] Clear(IWebDriver driver, string name, int delay, int unfollowAmount, int unfollowStart, int unfollowMinimum, bool shouldClear)
        {
            try{
                Console.WriteLine("Clearing followers!\nPreparing to unfollow...");
                driver.Navigate().GoToUrl("https://www.instagram.com/"+name);

                System.Threading.Thread.Sleep(Program.RandomSec(delay*2));

                int followingCount = int.Parse(driver.FindElement(By.XPath("//a[@href=\"/"+name+"/following/\"]/span")).Text.Replace(",", ""));
                int followerCount = int.Parse(driver.FindElement(By.XPath("//a[@href=\"/"+name+"/followers/\"]/span")).Text.Replace(",", ""));

                int[] stats = {0,0,0};
                int[] interactions = getAverageInteractions(driver, delay, name);
                stats[0] = followerCount;
                stats[1] = interactions[0];
                stats[2] = interactions[1];

                Console.WriteLine((int)(((float)((float)stats[1]+(float)stats[2])/(float)followerCount)*100f)+"%");

                driver.Navigate().GoToUrl("https://www.instagram.com/"+name);

                if(followingCount>unfollowMinimum&&shouldClear)
                {
                    System.Threading.Thread.Sleep(Program.RandomSec(delay));
                    driver.FindElement(By.XPath("//li[3]/a")).Click();
                    System.Threading.Thread.Sleep(Program.RandomSec(delay));
                    Console.WriteLine("Following "+followingCount+" people!\nUnfollowing...");
                    
                    try{
                        Actions actions = new Actions(driver);

                        actions.Click(driver.FindElement(By.XPath("//div[2]/ul/div/li[1]")));
                        actions.SendKeys(Keys.End).Build().Perform();

                        while(driver.FindElements(By.XPath("//div[2]/ul/div/li["+(unfollowStart+unfollowAmount+2)+"]")).Count == 0 )
                        {
                            actions.SendKeys(Keys.End).Build().Perform();
                        }

                        try{
                            for(int i=unfollowStart;i<unfollowStart+unfollowAmount;i++)
                            {
                                IWebElement unfollow = driver.FindElement(By.XPath("//div[2]/ul/div/li["+i+"]/div/div[2]/button"));
                                actions.MoveToElement(unfollow);
                                unfollow.Click();
                                driver.FindElement(By.CssSelector("button.aOOlW.-Cab_")).Click();
                            }
                        }
                        catch{}
                    }
                    catch{}
                }
                else
                { 
                    Console.WriteLine("Following "+followingCount+" people!\nNot unfollowing...");
                }
                return stats;
            }
            catch{}
        }
    
        // Likes first division of loaded recent posts
        static int[] Explore(IWebDriver driver, string tag, String[] comments, int skipRate, int followRate, int commentRate, int delay, int totalLike, int totalComment, int totalFollow, int maxLike, int maxFollow)
        {
            int[] stats = {0,0,0,0};

            try{driver.Navigate().GoToUrl("https://www.instagram.com/explore/tags/"+tag+"/");}
            catch{}
            System.Threading.Thread.Sleep(Program.RandomSec(delay*2));

            Console.WriteLine("Exploring posts with the tag " + tag + "...");

            Console.WriteLine(driver.Url);

            for(int y=1;y<6;y++)
            {
                for(int x=1;x<4;x++)
                {
                    IWebElement post = driver.FindElement(By.XPath("//div[2]/div/div[" + y.ToString() + "]/div[" + x.ToString() + "]/a/div[1]/div[2]"));

                     try
                {
                    Actions actions = new Actions(driver);

                    actions.MoveToElement(post).Build().Perform();
                    actions.Click(post).Build().Perform();
                    System.Threading.Thread.Sleep(Program.RandomSec(delay*2)); // Add delay to seem as if image has been seen

                    if(Program.Odds(skipRate)) // Adds randomness to simulate humans
                    {
                        Console.WriteLine("1");

                        if(totalLike+stats[1]<maxLike)
                        {
                            Console.WriteLine(driver.Url);
                            stats[1]++;
                            driver.FindElement(By.XPath("//section[1]/span[1]/button")).Click(); // Like any post that is not skipped
                            Console.WriteLine("Liked: " + driver.Url);

                            System.Threading.Thread.Sleep(Program.RandomSec(delay*1));
                        }

                        if(Program.Odds(followRate)&&totalFollow+stats[3]<maxFollow) // Reduces amount of people followed to avoid ban
                        {
                            Console.WriteLine("3");
                            stats[3]++;
                            
                            driver.FindElements(By.XPath("//button[text()='Follow']"))[1].Click();
                            Console.WriteLine("Followed!");
                        }
                        
                        if(Program.Odds(commentRate)&&totalComment+stats[2]<3) // Reduces spam to avoid ban
                        {
                            Console.WriteLine("4");
                            stats[2]++;
                            
                            driver.FindElement(By.XPath("//section[1]/span[2]/button")).Click();
                            actions.SendKeys(comments[new Random().Next(1, comments.Length)-1]+Keys.Enter);
                            System.Threading.Thread.Sleep(Program.RandomSec(delay*2)); // Adds delay to act as if typing
                            Console.WriteLine("Commented");                    
                        }
                    } 
                    stats[0]++;
                    driver.FindElement(By.XPath("//div[4]/div[3]/button")).Click(); // Close the post
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Reee");
                    break;
                }
                System.Threading.Thread.Sleep(Program.RandomSec(2));
                }
            }

            Console.WriteLine("Done!");
            return stats;
        }

        //Function to control and configure one cycle, measured in the start function
        public static void InteractCycles(IWebDriver driver, string[] tags, string name, string[] comments, int skipRate, int followRate, int commentRate, int delay, bool shouldClear, int maxLike, int maxFollow, int unfollowAmount, int unfollowStart, int unfollowMinimum, int tagsPerCycle)
        {
            // Stats[0] is total views (for debugging)
            // Stats[1] is total likes (for limits)
            // Stats[2] is total comments (for limits)
            // Stats[3] is total followed (for limits)
            // Stats[4] is average likes (for stats)
            // Stats[5] is average comments (for stats)
            int[] stats = {0,0,0,0,0,0};
            int followerCount;
            long startTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            int cyclePeriod = 900000;
            
            for(int x=0;x<tagsPerCycle/2;x++)
            {
                string tag = tags[new Random().Next(tags.Length)];
                int[] exploreStats = Explore(driver, tag, comments, skipRate, followRate, commentRate, delay, stats[1], stats[2], stats[3], maxLike, maxFollow);
                for(int i=0;i<4;i++){stats[i] = stats[i]+exploreStats[i];}  
            }

            try{
                int[] clearStats=Clear(driver, name, delay, unfollowAmount, unfollowStart, unfollowMinimum, shouldClear);
                followerCount = clearStats[0];
                stats[4] = clearStats[1];
                stats[5] = clearStats[2];
            }
            finally{}

            for(int x=0;x<tagsPerCycle/2;x++)
            {
                string tag = tags[new Random().Next(tags.Length)];
                int[] exploreStats = Explore(driver, tag, comments, skipRate, followRate, commentRate, delay, stats[1], stats[2], stats[3], maxLike, maxFollow);
                for(int i=0;i<4;i++){stats[i] = stats[i]+exploreStats[i];}  
                if(((DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond)-startTime)>=600000){break;} // Force explore to be less than 10 minutes
            }

            Console.WriteLine("F: "+followerCount+" V: "+stats[0]+" L: "+stats[1]+" C: "+stats[2]+" Fd: "+stats[3]+" E:"+(int)(((float)((float)stats[4]+(float)stats[5])/(float)followerCount)*100f)); 

            System.Threading.Thread.Sleep(Convert.ToInt32(startTime+cyclePeriod-(DateTime.Now.Ticks/TimeSpan.TicksPerMillisecond)));

            File.AppendAllText(@"stats/"+name+".txt",DateTime.Now.Year+"-"+DateTime.Now.Month+"-"+DateTime.Now.Day+" "+DateTime.Now.Hour+":"+DateTime.Now.Minute+"|"+followerCount+","+stats[4]+","+stats[5]+","+(int)(((float)((float)stats[4]+(float)stats[5])/(float)followerCount)*100f)+Environment.NewLine);
        }
    }
}