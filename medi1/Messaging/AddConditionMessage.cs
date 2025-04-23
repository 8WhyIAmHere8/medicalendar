using CommunityToolkit.Mvvm.Messaging.Messages;

public class AddConditionMessage : ValueChangedMessage<string>
{
    public AddConditionMessage(string value) : base(value)
    {
    }
}
