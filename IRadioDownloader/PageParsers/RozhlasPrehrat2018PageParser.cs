﻿using Dtc.Common.Extensions;
using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using RadioOwl.PageParsers.Base;
using RadioOwl.PageParsers.Data;
using RadioOwl.Radio;
using System;
using System.Linq;
using System.Threading.Tasks;
using vt.Http;

namespace RadioOwl.PageParsers
{
    public class RozhlasPrehrat2018PageParser : PageParserBase, IPageParser
    {
        public RozhlasPrehrat2018PageParser(IPageParser next = null) : base(next) { }


        /// <summary>
        /// Odkaz 'Přehrát' verze 2018-11
        /// http://plus.rozhlas.cz/host-galeristka-a-kuratorka-jirina-divacka-7671850?player=on#player
        /// http://region.rozhlas.cz/malebne-vlakove-nadrazi-v-hradku-u-susice-se-dostalo-mezi-deset-nejkrasnejsich-u-7671216?player=on#player
        /// http://radiozurnal.rozhlas.cz/pribeh-stoleti-7627378#dil=99?player=on#player
        /// http://dvojka.rozhlas.cz/miroslav-hornicek-petatricet-skvelych-pruvanu-a-jine-povidky-7670628#dil=2?player=on#player
        /// </summary>
        public override bool CanParseCondition(string url)
        {
            return !string.IsNullOrEmpty(url)
                            && url.StartsWith(@"http://", StringComparison.InvariantCultureIgnoreCase)
                            && url.Contains(@".rozhlas.cz/")
                            && url.EndsWith(@"?player=on#player", StringComparison.InvariantCultureIgnoreCase);
        }


        //public async Task<ParserResult> Parse(string url)
        //{
        //    var html = await DownloadHtml(url);

        //    if (string.IsNullOrEmpty(html))
        //        return new ParserResult(null).AddLog($"Nepodařilo se stažení hlavní stránky: '{url}'"); // no source html - no fun
        //    //Parse(parserResult);
        //    else
        //        return ParseHtml(html); // try to parse
        //}


        //        private async Task<string> DownloadHtml(string url)
        //        {
        //            var asyncDownloader = new AsyncDownloader();
        //            var downloaderOutput = await asyncDownloader.GetString(url);
        //            return downloaderOutput.DownloadOk ? downloaderOutput.Output : null;


        //            {
        //                //return new ParserResult(downloaderOutput.Output);
        //            }
        ////            return new ParserResult(null).AddLog($"Nepodařilo se stažení hlavní stránky: '{url}'");
        //        }


        //private async Task<ParserResult> TryDecodeUrl(string url)
        //{
        //    var asyncDownloader = new AsyncDownloader();
        //    var downloader = await asyncDownloader.GetString(url);
        //    if (downloader.DownloadOk)
        //    {
        //        return Parse(downloader.Output);
        //    }
        //    else
        //    {
        //        return new ParserResult().AddLog($"Nepodařilo se stažení: '{url}'");
        //    }
        //}


        public override ParserResult ParseHtml(string html)
        {
            var parserResult = new ParserResult(html);
            try
            {
                // html nemusi byt validni xml, takze je potreba pro parsovani pouzit Html Agility Pack
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(parserResult.SourceHtml);

                // get all  <script> under <head>
                var headScriptSet = htmlDoc.DocumentNode.SelectNodes(@"//head//script");
                if (headScriptSet != null && headScriptSet.Any())
                {
                    // < div class="sm2-playlist-wrapper">


                    var divSetXXX = htmlDoc.DocumentNode.SelectNodes(@"//div[@part and class='sm2-row sm2-wide']");


                    var divPlaylistSet = htmlDoc.DocumentNode.SelectNodes(@"//div[@class='sm2-playlist-wrapper']");


                    var mp3AnchorSet = htmlDoc.DocumentNode.SelectNodes(@"//div[@class='sm2-playlist-wrapper']//ul//li//div//a");

                    Get(mp3AnchorSet, ref parserResult);


//                    if (divPlaylistSet != null && divPlaylistSet.Any())
//                    {
//                        foreach(var divPlaylistRoot in divPlaylistSet)
//                        {

//                          // Get(divPlaylistRoot, ref parserResult);

//                            //var divSet = divPlaylistRoot.SelectNodes(@".//div[@part and class='sm2 - row sm2 - wide']");


//                            //var div1 = divPlaylistRoot.ChildNodes.FirstOrDefault(p => p.Attributes["part"] != null);
                                
////                                ...SelectNodes(@".//div[@part]");

//                        }
//                    }




                    var drupalSettingsJson = headScriptSet.FirstOrDefault(p => p.InnerText.Contains("jQuery.extend(Drupal.settings"))?.InnerText;
                    if (!string.IsNullOrEmpty(drupalSettingsJson))
                    {
                        // select inner json data from <script> element
                        var json = drupalSettingsJson.RemoveStartTextTo('{').RemoveEndTextTo('}');
                        json = "{" + json + "}";

                        var jObject = JObject.Parse(json);

                        //  ajaxPageState "soundmanager2":{
                        var downloadItem = jObject.SelectToken("soundmanager2.playtime");
                        if (downloadItem != null)
                        {
                            foreach (JToken item in downloadItem.Children())
                            {
                                // takhle to vypada: "https://region.rozhlas.cz/sites/default/files/audios/68919bf46b77f6246089a1dd38b35bf9.mp3": "https://region.rozhlas.cz/audio-download/sites/default/files/audios/68919bf46b77f6246089a1dd38b35bf9-mp3"
                                // mp3 se da stahnout z obou url ... zatim tedy budu pouzivat ten prvni
                                var urlToken = item.ToString();
                                if (!string.IsNullOrEmpty(urlToken))
                                {
                                    var urlSet = urlToken.Split('"');
                                    if (urlSet.Count() > 2)
                                    {
                                        parserResult.AddUrl(urlSet[1], "");
                                    }
                                }
                            }
                        }

                        // nektere 'prehrat' html stranky nemaji prehravac s json daty a mp3 url musim dohledat jinde ve strance
                        if (!parserResult.RozhlasUrlSet.Any())
                        {
                            // najit prislusny div
                            var parentDiv = htmlDoc.DocumentNode.SelectSingleNode(@"//div[@aria-labelledby='Audio part']");
                            // pod nim by mel byt jeden <a> s href atributem - url k mp3
                            if (parentDiv != null)
                            {
                                var aHref = parentDiv.ChildNodes.FirstOrDefault(p => p.Name == "a")?.Attributes["href"]?.Value;
                                if (!string.IsNullOrEmpty(aHref))
                                {
                                    parserResult.AddUrl(aHref, null);
                                }
                            }
                        }

                        // po vsechn pokusech nic nenalezeno?
                        if (parserResult.RozhlasUrlSet.Any())
                        {
                            // title jen vykousnu ze stranky
                            GetTitleFromH1(htmlDoc, ref parserResult);
                        }
                        else
                        {
                            parserResult.AddLog("Chyba při parsování html - nepodařilo se dohledat seznam url z json dat.");
                        }
                    }
                    else
                    {
                        parserResult.AddLog("Chyba při parsování html - nepodařilo se dohledat 'Drupal.Setings' json data.");
                    }
                }
                else
                {
                    parserResult.AddLog("Chyba při parsování html - nepodařilo se dohledat //head//script nody.");
                }
            }
            catch (Exception ex)
            {
                parserResult.AddLog($"ParsePrehrat2018Html error: {ex.Message}.");
            }

            return parserResult;
        }

        private void Get(HtmlNodeCollection mp3AnchorSet, ref ParserResult parserResult)
        {
            /*
                <div class="sm2-playlist-wrapper">
                      <ul class="sm2-playlist-bd">
                                                  <li>
												  <div part="1" class="sm2-row sm2-wide" id="file-8490384">
						===>  					    <a href="https://vltava.rozhlas.cz/sites/default/files/audios/8823b0fd947daa76167e9014d6ed4014.mp3?uuid=5c17536947ad0">
												        <div class="filename" title="Steinar Bragi: Planina">
												            <div class="filename__text" title="Steinar Bragi: Planina">1. díl: Steinar Bragi: Planina</div>
												        </div>
												    </a>
												  <div class="audio-info-wrap">
												  <span class="playlist-audio-time-to-expire">
												  <span class="caption__desktop-only">k poslechu </span>ještě 3 dny</span>
												  <span class="playlist-audio-length">28:14</span>
												  </div>
												  </div>
												  </li>              
             */

            if (parserResult == null)
                return;

            if (mp3AnchorSet != null || mp3AnchorSet.Any())
            {
                foreach(var mp3A in mp3AnchorSet)
                {
                    var rozhlasUrl = new RozhlasUrl();
                    rozhlasUrl.Url = mp3A.Attributes["href"]?.Value;

                    var subDiv = mp3A.Descendants("div")?.FirstOrDefault()?.Descendants("div")?.FirstOrDefault();

                    if(subDiv!= null)
                    {
                        var title = subDiv.Attributes["title"]?.Value ?? "TmpTitle";
                        var text = subDiv.InnerText ?? "TmpText";



// TODO mozna rovnou prevadet na filename? a nezkoumat to dal???



                        rozhlasUrl.Description = "";
                    }
                    else
                    {
                        parserResult.AddLog($"ParsePrehrat2018Html - subDiv is null.");
                    }



                }

                //var ulRootItem = divPlaylistRoot.ChildNodes.FirstOrDefault(p => p.Name == "ul" && p.Attributes["class"]?.Value == "sm2-playlist-bd");
                //if (ulRootItem != null)
                //{
                //    var liSet = ulRootItem.ChildNodes.Where(p => p.Name == "li").ToList();
                //    if (liSet != null && liSet.Any())
                //    {
                //        foreach (var li in liSet)
                //        {

                //            var mp3Url = li.Descendants("a").FirstOrDefault();

                //        }

                //    }
                //    else
                //    {
                //        parserResult.AddLog($"ParsePrehrat2018Html - liSet is empty.");
                //    }
                //}
                //else
                //{
                //    parserResult.AddLog($"ParsePrehrat2018Html - ulRootItem is null.");
                //}
            }
            else
            {
                parserResult.AddLog($"ParsePrehrat2018Html - mp3AnchorSet is null.");
            }










            //if (divPlaylistRoot != null)
            //{
            //    var ulRootItem = divPlaylistRoot.ChildNodes.FirstOrDefault(p => p.Name == "ul" && p.Attributes["class"]?.Value == "sm2-playlist-bd");
            //    if (ulRootItem != null)
            //    {
            //        var liSet = ulRootItem.ChildNodes.Where(p => p.Name == "li").ToList();
            //        if (liSet != null && liSet.Any())
            //        {
            //            foreach (var li in liSet)
            //            {

            //                var mp3Url = li.Descendants("a").FirstOrDefault();

            //            }

            //        }
            //        else
            //        {
            //            parserResult.AddLog($"ParsePrehrat2018Html - liSet is empty.");
            //        }
            //    }
            //    else
            //    {
            //        parserResult.AddLog($"ParsePrehrat2018Html - ulRootItem is null.");
            //    }
            //}
            //else
            //{
            //    parserResult.AddLog($"ParsePrehrat2018Html - divPlaylistRoot is null.");
            //}
        }


        /// <summary>
        /// dohledani informaci o poradu z meta tagu html
        /// </summary>
        private void GetTitleFromH1(HtmlDocument htmlDoc, ref ParserResult parserResult)
        {
            // TODO description ke vsemu stejne? nebo se podari vykousat jednotlive dily?

            var title = GetMetaTagContent(htmlDoc, @"//meta[@property='og:title']");
            // <meta property="og:description" content="Poslechněte si oblíbené poetické texty básníka a publicisty Milana Šedivého." />
            var description = GetMetaTagContent(htmlDoc, @"//meta[@property='og:description']");
            // <meta property="og:site_name" content="Vltava" />
            var siteName = GetMetaTagContent(htmlDoc, @"//meta[@property='og:site_name']");

            parserResult.RozhlasUrlSet.ForEach(
                p => 
                {
                    p.Title = title;
                    p.Description = description;
                    p.SiteName = siteName;
                }
            );
        }


        private string GetMetaTagContent(HtmlDocument htmlDoc, string xPath)
        {
            var xpathNodes = htmlDoc.DocumentNode.SelectNodes(xPath);
            var contentAttribute = xpathNodes?.FirstOrDefault()?.Attributes["content"]?.Value;

            // dencode char such &nbsp; as well (https://stackoverflow.com/questions/6665488/htmlagilitypack-and-htmldecode)
            var deEntitized = HtmlEntity.DeEntitize(contentAttribute);
            return deEntitized;
        }
    }
}
