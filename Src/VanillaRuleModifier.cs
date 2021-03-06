﻿using System;
using System.Linq;
using System.Net;
using AngleSharp.Dom.Html;
using AngleSharp.Extensions;
using AngleSharp.Parser.Html;
using RT.Servers;
using VanillaRuleGenerator;

namespace KtaneWeb
{
    public sealed partial class KtanePropellerModule
    {
        private HttpResponse vanillaRuleModifier(HttpRequest req)
        {
	        var manualGenerator = new ManualGenerator(_config.VanillaRuleModifierMods, _config.VanillaRuleModifierCache);

            if (!int.TryParse(req.Url["VanillaRuleSeed"], out int seed) && !int.TryParse(req.Url["RuleSeed"], out seed))
                return manual(req);

            string path = req.Url.Path.Substring(1);

            string modifiedmanual;

            if (path == "")
            {
                modifiedmanual = $"<html><head><title>Repository of Manual pages</title></head><body><h1>Seed = {seed}</h1><h2>Vanilla Manuals</h2>";
	            modifiedmanual += $@"<a href=""index.html?VanillaRuleSeed={seed}"">Full Vanilla Manual</a><ul>";

	            modifiedmanual = manualGenerator.GetVanillaHTMLFileNames()
		            .Aggregate(modifiedmanual, (current, html) => current + $"<li><a href=\"{WebUtility.UrlEncode(html)}?VanillaRuleSeed={seed}\">{html}</a></li>");

	            modifiedmanual += @"</ul><h2>Mod Manuals</h2><ul>";

				modifiedmanual = manualGenerator.GetModHTMLFileNames()
                    .Aggregate(modifiedmanual, (current, html) => current + $"<li><a href=\"{WebUtility.UrlEncode(html)}?VanillaRuleSeed={seed}\">{html}</a></li>");

                modifiedmanual += "</ul></body></html>";
            }
            else
            {
                modifiedmanual = manualGenerator.GetHTMLManual(seed, WebUtility.UrlDecode(path), !string.IsNullOrEmpty(_config.VanillaRuleModifierCache));
            }

            IHtmlDocument document = new HtmlParser().Parse(modifiedmanual);

            if (document.Scripts.All(x => x.GetAttribute("src") != "js/highlighter.js"))
            {
                var highlighter = document.CreateElement<IHtmlScriptElement>();
                highlighter.Source = "js/highlighter.js";
                document.Head.Append(highlighter);
            }

            return HttpResponse.Html(document.ToHtml());
        }
    }
}