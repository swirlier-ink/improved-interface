using Terraria.UI;

namespace ImprovedInterface.Common;

public static class UserInterfaceExtensions
{
    extension(UserInterface @interface)
    {
        public UIState? State
        {
            get => @interface.CurrentState;

            set
            {
                if (@interface.State != value)
                {
                    @interface.SetState(value);
                }
            }
        }
    }
}
