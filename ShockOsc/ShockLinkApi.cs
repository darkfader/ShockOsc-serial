﻿using Serilog;
using System.Net;
using System.Text.Json;

namespace OpenShock.ShockOsc;

public class ShockLinkApi
{
    private static readonly ILogger Logger = Log.ForContext(typeof(ShockLinkApi));

    private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
    };

    public static List<Device.Shocker> Shockers = new();

    public static async Task GetShockers()
    {
        var response = await WebRequestApi.DoRequest(new WebRequestApi.RequestData
        {
            url = $"{Config.ConfigInstance.ShockLink.OpenShockApi}/shockers/own"
        });
        if (response.Item1 == HttpStatusCode.OK)
        {
            Shockers.Clear();
            var shockers = JsonSerializer.Deserialize<OwnShockersResponseResponseData>(response.Item2, _jsonOptions);
            if (shockers == null || shockers.Data.Length == 0)
            {
                Logger.Error("Failed to deserialize shockers: {response}", response);
                return;
            }
            foreach (var device in shockers.Data)
            {
                foreach (var shocker in device.Shockers)
                {
                    Shockers.Add(shocker);
                }
            }

            // populate config
            var shockerList = new Dictionary<string, Guid>();
            foreach (var shocker in Shockers)
            {
                shockerList.Add(shocker.Name, shocker.Id);
            }
            Config.ConfigInstance.ShockLink.Shockers = shockerList;
            Config.Save();
        }
        else
        {
            Logger.Error("Failed to fetch shockers: {response}", response);
        }
    }
}