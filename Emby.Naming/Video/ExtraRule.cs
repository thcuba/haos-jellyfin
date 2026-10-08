using System.Text.RegularExpressions;
using MediaBrowser.Model.Entities;
using MediaType = Emby.Naming.Common.MediaType;

namespace Emby.Naming.Video
{
    /// <summary>
    /// A rule used to match a file path with an <see cref="MediaBrowser.Model.Entities.ExtraType"/>.
    /// </summary>
    public class ExtraRule
    {
        private string _token;
        private ExtraRuleType _ruleType;
        private Regex? _regex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtraRule"/> class.
        /// </summary>
        /// <param name="extraType">Type of extra.</param>
        /// <param name="ruleType">Type of rule.</param>
        /// <param name="token">Token.</param>
        /// <param name="mediaType">Media type.</param>
        public ExtraRule(ExtraType extraType, ExtraRuleType ruleType, string token, MediaType mediaType)
        {
            _token = token;
            ExtraType = extraType;
            _ruleType = ruleType;
            MediaType = mediaType;
        }

        /// <summary>
        /// Gets or sets the token to use for matching against the file path.
        /// </summary>
        public string Token
        {
            get => _token;
            set
            {
                if (_token != value)
                {
                    _token = value;
                    _regex = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the extra to return when matched.
        /// </summary>
        public ExtraType ExtraType { get; set; }

        /// <summary>
        /// Gets or sets the type of the rule.
        /// </summary>
        public ExtraRuleType RuleType
        {
            get => _ruleType;
            set
            {
                if (_ruleType != value)
                {
                    _ruleType = value;
                    _regex = null;
                }
            }
        }

        /// <summary>
        /// Gets or sets the type of the media to return when matched.
        /// </summary>
        public MediaType MediaType { get; set; }

        /// <summary>
        /// Gets a compiled <see cref="Regex"/> instance for <see cref="ExtraRuleType.Regex"/> rules (cached for performance).
        /// </summary>
        public Regex? Regex
        {
            get
            {
                if (_ruleType != ExtraRuleType.Regex)
                {
                    return null;
                }

                return _regex ??= new Regex(_token, RegexOptions.IgnoreCase | RegexOptions.Compiled);
            }
        }
    }
}
