using Parser.Events;
using Parser.Matchers;

namespace Parser;

public class GreToClientEventMatcher : ILogMatcher
{
    public event EventHandler<HoldFullControlChangeEvent>? FullControlChangeEvent;

    public bool Match(string line)
    {
        line = line.Trim();
        if (!line.Contains(TRANSACTION_ID_NEEDLE) || !line.Contains(REQUEST_ID_NEEDLE) || !line.Contains(GRE_TO_CLIENT_EVENT_NEEDLE))
        {
            return false;
        }
        
        var e = ParseFullControlChangeInfo(line);
        if(e == null)
        {
            return false;  
        }

        FullControlChangeEvent?.Invoke(this, e);

        return true;        
    }

    private static HoldFullControlChangeEvent? ParseFullControlChangeInfo(string line)
    {
        dynamic? e = MatcherHelpers.ParseGreToClientEvent(line);
        var messages = e?.greToClientMessages;
        if(messages == null)
        {
            return null; 
        }

        bool? holdFullControl = null;
        foreach (var message in messages)
        {
            var type = message.type;
            if (type == null || type != GRE_MESSAGE_TYPE_NEEDLE)
            {
                continue;
            }

            var settings = message?.setSettingsResp?.settings;
            if(settings == null)
            {
                continue;
            }

            var holdFullControlSetting = settings.defaultAutoPassOption;
            if (holdFullControlSetting != null)
            {
                holdFullControl = holdFullControlSetting == FULL_CONTROL_HOLD_VALUE_NEEDLE;
            }
        }

        if(holdFullControl == null)
        {
            return null; 
        }

        return new(holdFullControl.Value);
    }

    private static readonly string TRANSACTION_ID_NEEDLE = "transactionId";
    private static readonly string REQUEST_ID_NEEDLE = "requestId";
    private static readonly string GRE_TO_CLIENT_EVENT_NEEDLE = "greToClientEvent";
    private static readonly string GRE_MESSAGE_TYPE_NEEDLE = "GREMessageType_SetSettingsResp";
    private static readonly string FULL_CONTROL_HOLD_VALUE_NEEDLE = "AutoPassOption_FullControl";
}
