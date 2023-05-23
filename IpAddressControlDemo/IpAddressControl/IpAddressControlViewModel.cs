
namespace IpAddressControl
{
    using System;
    
    public class IpAddressViewModel : BaseViewModel
    {
        private bool isPart1Focused;
        private bool isPart2Focused;
        private bool isPart3Focused;
        private bool isPart4Focused;

        private string part1;
        private string part2;
        private string part3;
        private string part4;

        public event EventHandler AddressChanged;

        public string AddressText
        {
            get { return $"{Part1}.{Part2}.{Part3}.{Part4}"; }
        }

        public bool IsPart1Focused
        {
            get => isPart1Focused;
            set => OnPropertyChanged(ref isPart1Focused, value);
        }

        public bool IsPart2Focused
        {
            get => isPart2Focused;
            set => OnPropertyChanged(ref isPart2Focused, value);
        }

        public bool IsPart3Focused
        {
            get => isPart3Focused;
            set => OnPropertyChanged(ref isPart3Focused, value);
        }

        public bool IsPart4Focused
        {
            get => isPart4Focused;
            set => OnPropertyChanged(ref isPart4Focused, value);
        }

        public string Part1
        {
            get { return part1; }
            set
            {
                part1 = value;
                SetFocus(true, false, false, false);

                var moveNext = CanMoveNext(ref part1);
                AddressChanged?.Invoke(this, EventArgs.Empty);

                if (moveNext)
                {
                    SetFocus(false, true, false, false);
                }
            }
        }

        public string Part2
        {
            get { return part2; }
            set
            {
                part2 = value;
                SetFocus(false, true, false, false);

                var moveNext = CanMoveNext(ref part2);
                AddressChanged?.Invoke(this, EventArgs.Empty);

                if (moveNext)
                {
                    SetFocus(false, false, true, false);
                }
            }
        }

        public string Part3
        {
            get { return part3; }
            set
            {
                part3 = value;
                SetFocus(false, false, true, false);
                var moveNext = CanMoveNext(ref part3);
                AddressChanged?.Invoke(this, EventArgs.Empty);

                if (moveNext)
                {
                    SetFocus(false, false, false, true);
                }
            }
        }

        public string Part4
        {
            get { return part4; }
            set
            {
                part4 = value;
                SetFocus(false, false, false, true);
                var moveNext = CanMoveNext(ref part4);
                AddressChanged?.Invoke(this, EventArgs.Empty);
            }
        }
       
        public void SetAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return;

            var parts = address.Split('.');           

            if (int.TryParse(parts[0], out var num0))
            {
                Part1 = num0.ToString();
            }

            if (int.TryParse(parts[1], out var num1))
            {
                Part2 = parts[1];
            }

            if (int.TryParse(parts[2], out var num2))
            {
                Part3 = parts[2];
            }

            if (int.TryParse(parts[3], out var num3))
            {
                Part4 = parts[3];
            }

        }

        private bool CanMoveNext(ref string part)
        {
            bool moveNext = false;

            if (!string.IsNullOrWhiteSpace(part))
            {
                if (part.Length >= 3)
                {
                    moveNext = true;
                }

                if (part.EndsWith("."))
                {
                    moveNext = true;
                    part = part.Replace(".", "");
                }
            }

            return moveNext;
        }

        private void SetFocus(bool part1, bool part2, bool part3, bool part4)
        {
            IsPart1Focused = part1;
            IsPart2Focused = part2;
            IsPart3Focused = part3;
            IsPart4Focused = part4;
        }

    }
}
