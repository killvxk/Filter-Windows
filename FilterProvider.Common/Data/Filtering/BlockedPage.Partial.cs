using System;
using System.Collections.Generic;
using System.Text;
using Citadel.IPC.Messages;
using Filter.Platform.Common.Data.Models;

namespace FilterProvider.Common.Data.Filtering
{
    public class BlockedPageData
    {
        public MappedFilterListCategoryModel MatchingCategory { get; set; }
        public int MatchingCategoryId { get; set; }
        public List<MappedFilterListCategoryModel> AppliedCategories { get; set; }

        public BlockType Type { get; set; }

        public bool CanRequestUnblock()
        {
            switch(Type)
            {
                default:
                    return true;

                case BlockType.TimeRestriction:
                    return false;
            }
        }
    }

    public partial class BlockedPage : BlockedPageBase
    {
        private BlockedPageData data;

        public BlockedPage(BlockedPageData data)
        {
            this.data = data;
        }
    }
}
