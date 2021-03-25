using DSharpPlus;
using DSharpPlus.Entities;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using UnityEngine;
using VRC.Core;

[assembly: MelonLoader.MelonGame("VRChat", "VRChat")]
[assembly: MelonLoader.MelonInfo(typeof(AvatarLoger.jews), "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee", "eeeeeeeeeeeeeeeeeeeee", "KeeeeeeeeeeeeeeeeeeeeeeeafyIsHere")]

namespace AvatarLoger
{

    public class jews : MelonLoader.MelonMod
    {
        const string PublicAvatarFile = "AvatarLog\\Public.txt";
        const string PrivateAvatarFile = "AvatarLog\\Private.txt";

        static string AvatarIDs = "";
        static List<ApiAvatar> AvatarToPost = new List<ApiAvatar>();
        static Config config { get; set; }

        static HttpClient PostingClient = new HttpClient(); //Keep this here.


        private static HarmonyMethod GetPatch(string name) => new HarmonyMethod(typeof(jews).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        public override void OnApplicationStart()
        {

            Directory.CreateDirectory("AvatarLog");
            if (!File.Exists(PublicAvatarFile))
                File.AppendAllText(PublicAvatarFile, $"Made by KeafyIsHere{Environment.NewLine}");
            if (!File.Exists(PrivateAvatarFile))
                File.AppendAllText(PrivateAvatarFile, $"Made by KeafyIsHere{Environment.NewLine}");

            foreach (string line in File.ReadAllLines(PublicAvatarFile))
                if (line.Contains("Avatar ID"))
                    AvatarIDs += line.Replace("Avatar ID:", "");
            foreach (string line in File.ReadAllLines(PrivateAvatarFile))
                if (line.Contains("Avatar ID"))
                    AvatarIDs += line.Replace("Avatar ID:", "");

            if (!File.Exists("AvatarLog\\Config.json"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Config.json not found!");
                Console.WriteLine("Config.json Generating new one please fill out");
                File.WriteAllText("AvatarLog\\Config.json", JsonConvert.SerializeObject(new Config
                {
                    CanPostFriendsAvatar = false,
                    PrivateWebhook = "",
                    PublicWebhook = ""
                }, Formatting.Indented));
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Config File Detected!");
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("AvatarLog\\Config.json"));
            }


            HarmonyLib.Harmony patchman = HarmonyLib.Harmony.Create("pog");
            patchman.Patch(typeof(AssetBundleDownloadManager).GetMethods().Where(mi => mi.GetParameters().Length == 1 && mi.GetParameters()[0].ParameterType == typeof(ApiAvatar) && mi.ReturnType == typeof(void)).FirstOrDefault(), GetPatch("apiavatardownloadthingy"));

            MelonLoader.MelonCoroutines.Start(DoCheckV2());
        }
        private static bool apiavatardownloadthingy(ApiAvatar __0)
        {
            if (!AvatarIDs.Contains(__0.id))
            {
                if (__0.releaseStatus == "public")
                {
                    AvatarIDs += __0.id;
                    StringBuilder avatarlog = new StringBuilder();
                    avatarlog.AppendLine($"Avatar ID:{__0.id}");
                    avatarlog.AppendLine($"Avatar Name:{__0.name}");
                    avatarlog.AppendLine($"Avatar Description:{__0.description}");
                    avatarlog.AppendLine($"Avatar Author ID:{__0.authorId}");
                    avatarlog.AppendLine($"Avatar Author Name:{__0.authorName}");
                    avatarlog.AppendLine($"Avatar Asset URL:{__0.assetUrl}");
                    avatarlog.AppendLine($"Avatar Image URL:{__0.imageUrl}");
                    avatarlog.AppendLine($"Avatar Thumbnail Image URL:{__0.thumbnailImageUrl}");
                    avatarlog.AppendLine($"Avatar Release Status:{__0.releaseStatus}");
                    avatarlog.AppendLine($"Avatar Version:{__0.version}");
                    avatarlog.AppendLine(Environment.NewLine);
                    File.AppendAllText(PublicAvatarFile, avatarlog.ToString());
                    if (!string.IsNullOrEmpty(config.PublicWebhook) && CanPost(__0.id))
                        AvatarToPost.Add(__0);
                }
                else
                {
                    AvatarIDs += __0.id;
                    StringBuilder avatarlog = new StringBuilder();
                    avatarlog.AppendLine($"Avatar ID:{__0.id}");
                    avatarlog.AppendLine($"Avatar Name:{__0.name}");
                    avatarlog.AppendLine($"Avatar Description:{__0.description}");
                    avatarlog.AppendLine($"Avatar Author ID:{__0.authorId}");
                    avatarlog.AppendLine($"Avatar Author Name:{__0.authorName}");
                    avatarlog.AppendLine($"Avatar Asset URL:{__0.assetUrl}");
                    avatarlog.AppendLine($"Avatar Image URL:{__0.imageUrl}");
                    avatarlog.AppendLine($"Avatar Thumbnail Image URL:{__0.thumbnailImageUrl}");
                    avatarlog.AppendLine($"Avatar Release Status:{__0.releaseStatus}");
                    avatarlog.AppendLine($"Avatar Version:{__0.version}");
                    avatarlog.AppendLine(Environment.NewLine);
                    File.AppendAllText(PrivateAvatarFile, avatarlog.ToString());
                    if (!string.IsNullOrEmpty(config.PrivateWebhook) && CanPost(__0.id))
                        AvatarToPost.Add(__0);
                }
            }
            return true;
        }
        static bool CanPost(string id)
        {
            if (config.CanPostFriendsAvatar)
                return true;
            else if (APIUser.CurrentUser.friendIDs.Contains(id))
                return false;
            return true;
        }

        IEnumerator DoCheckV2()
        {
            for (; ; )
            {
                try
                {
                    if (AvatarToPost.Count > 0)
                    {

                        RestWebhookExecutePayload whplprivate = new RestWebhookExecutePayload
                        {
                            Content = "",
                            Username = string.IsNullOrEmpty(config.BotName) ? "Loggy boi" : config.BotName,
                            AvatarUrl = string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL,
                            IsTTS = false,
                            Embeds = new List<DiscordEmbed>()
                        };

                        RestWebhookExecutePayload whplpublic = new RestWebhookExecutePayload
                        {
                            Content = "",
                            Username = string.IsNullOrEmpty(config.BotName) ? "Loggy boi" : config.BotName,
                            AvatarUrl = string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL,
                            IsTTS = false,
                            Embeds = new List<DiscordEmbed>()
                        };

                        for (int i = 0; i < AvatarToPost.Count; i++)
                        {
                            ApiAvatar avi = AvatarToPost[i];
                            if (avi != null)
                            {
                                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();
                                discordEmbed.WithAuthor(string.IsNullOrEmpty(config.BotName) ? "Loggy boi" : config.BotName, string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL, string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL);
                                discordEmbed.WithImageUrl(avi.thumbnailImageUrl);
                                discordEmbed.WithColor(new DiscordColor(avi.releaseStatus == "public" ? "#00FF00" : "#FF0000"));
                                discordEmbed.WithUrl($"https://vrchat.com/api/1/avatars/{avi.id}?apiKey=JlE5Jldo5Jibnk5O5hTx6XVqsJu4WJ26");
                                discordEmbed.WithTitle("Click Me (API Link)");
                                discordEmbed.WithDescription("Must be logged in on VRChat.com to view api link ^^");
                                discordEmbed.WithTimestamp(DateTimeOffset.Now);
                                discordEmbed.AddField("Avatar ID:", avi.id);
                                discordEmbed.AddField("Avatar Name:", avi.name);
                                discordEmbed.AddField("Avatar Description:", avi.description);
                                discordEmbed.AddField("Avatar Author ID:", avi.authorId);
                                discordEmbed.AddField("Avatar Author Name:", avi.authorName);
                                discordEmbed.AddField("Avatar Version:", avi.version.ToString());
                                discordEmbed.AddField("Avatar Release Status:", avi.releaseStatus);
                                discordEmbed.AddField("Avatar Asset URL:", avi.assetUrl);
                                discordEmbed.AddField("Avatar Image URL:", avi.imageUrl);
                                discordEmbed.AddField("Avatar Thumbnail Image URL:", avi.thumbnailImageUrl);
                                discordEmbed.WithFooter("Made by KeafyIsHere", string.IsNullOrEmpty(config.AvatarURL) ? "https://i.imgur.com/No3R2yY.jpg" : config.AvatarURL);
                                if (APIUser.IsFriendsWith(avi.authorId) && !config.CanPostFriendsAvatar)
                                {
                                    if (avi.releaseStatus == "public")
                                        whplpublic.Embeds.Add(discordEmbed.Build());
                                    else
                                        whplprivate.Embeds.Add(discordEmbed.Build());
                                }
                                
                                if (whplprivate.Embeds.Count > 23) // Max 25 embeds but keep it 23 so theres no weird like glitches (it just helps ok ok)
                                {
                                    PostingClient.PostAsync(config.PrivateWebhook, new StringContent(JsonConvert.SerializeObject(whplprivate), Encoding.UTF8, "application/json"));
                                    whplprivate.Embeds.Clear();
                                }

                                if (whplpublic.Embeds.Count > 23)// Max 25 embeds but keep it 23 so theres no weird like glitches (it just helps ok ok)
                                {
                                    PostingClient.PostAsync(config.PrivateWebhook, new StringContent(JsonConvert.SerializeObject(whplpublic), Encoding.UTF8, "application/json"));
                                    whplpublic.Embeds.Clear();
                                }
                            }
                            AvatarToPost.Remove(avi);
                        }

                        if (whplprivate.Embeds.Count > 0)
                            PostingClient.PostAsync(config.PrivateWebhook, new StringContent(JsonConvert.SerializeObject(whplprivate), Encoding.UTF8, "application/json"));



                        if (whplpublic.Embeds.Count > 0)
                            PostingClient.PostAsync(config.PublicWebhook, new StringContent(JsonConvert.SerializeObject(whplpublic), Encoding.UTF8, "application/json"));

                    }
                }
                catch (Exception ex)
                {
                    MelonLoader.MelonLogger.Error(ex);
                }
                yield return new WaitForSeconds(1f);
            }


            yield return new WaitForSeconds(1f);
        }




    }
}
