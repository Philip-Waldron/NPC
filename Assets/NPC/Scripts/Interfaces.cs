namespace NPC.Scripts
{
    public interface IScan
    {
        void Scanned();
    }
    
    public interface ISap<in T>
    {
        void Sapped(T sap);
    }
    
    public interface IShoot
    {
        void Shot();
    }

    public interface IEmote<in T, in X>
    {
        void Emote(T emoteIndex, X duration);
    }
    
    public interface ISpeak<in T, in X, in D>
    {
        void SpeakText(T speechText, X duration);
        void SpeakAudio(D audioClipIndex);
    }
    
    public interface IInteractable<in T>
    {
       void Pickup(T player);
    }

    public interface IEquipment
    {
        void UseEquipment();
    }
}
