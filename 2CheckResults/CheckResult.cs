using PortalFrame._1Model;
using PortalFrame._3Dispatch;

namespace PortalFrame._2CheckResults
{
    public class CheckResult
    {
        public GeneralPara General = new();
        // 与组合无关的结果
        public SlendernessResult Slenderness = new();
        public LocalStabilityResult LocalStability = new();

        // 与组合有关的强度验算
        public FlexureResult FlexureStrength = new();
        public ShearResult ShearStrength = new();

        //整体受弯稳定系数
        public PhiBResult PhiBResult = new();

        // 柱专属
        public ColumnInPlaneStabilityResult InPlaneStability = new();
        public ColumnOutPlaneStabilityResult OutPlaneStability = new();

        // 梁专属
        public BeamLateralStabilityResult LateralStability = new();

        // 最不利结果
        public WorstResult Worst = new();

        // 通用部分（柱梁都算这些）
        public virtual void GetWorstResult(MemberData member)
        {
            Worst.MaxUtil = 0;
            Worst.BestCombo = "";
            Worst.CheckType = "";
            Worst.Station = null;

            // 1. 抗弯最不利
            var (flexMax, flexCombo, flexStation) = FlexureStrength.GetWorst();
            if (flexMax > Worst.MaxUtil)
            {
                Worst.MaxUtil = flexMax;
                Worst.BestCombo = flexCombo;
                Worst.CheckType = CheckTypes.Flexure;
                Worst.Station = flexStation;
            }

            // 2. 抗剪最不利
            var (shearMax, shearCombo, shearStation) = ShearStrength.GetWorst();
            if (shearMax > Worst.MaxUtil)
            {
                Worst.MaxUtil = shearMax;
                Worst.BestCombo = shearCombo;
                Worst.CheckType = CheckTypes.Shear;
                Worst.Station = shearStation;
            }

            // 3. 稳定最不利（面内、面外、梁整体稳定）
            if(member.Type == MemberType.Column)
            {
                double inPlaneMax = InPlaneStability.MaxUtil;
                if (inPlaneMax > Worst.MaxUtil)
                {
                    Worst.MaxUtil = inPlaneMax;
                    Worst.BestCombo = InPlaneStability.WorstCombo;
                    Worst.CheckType = CheckTypes.ColumnInPlane;
                }

                double outPlaneMax = OutPlaneStability.MaxUtil;
                if (outPlaneMax > Worst.MaxUtil)
                {
                    Worst.MaxUtil = outPlaneMax;
                    Worst.BestCombo = OutPlaneStability.WorstCombo;
                    Worst.CheckType = CheckTypes.ColumnOutPlane;
                }
            }
            if(member.Type == MemberType.Beam)
            {
                double lateralMax = LateralStability.MaxUtil;
                if (lateralMax > Worst.MaxUtil)
                {
                    Worst.MaxUtil = lateralMax;
                    Worst.BestCombo = LateralStability.WorstCombo;
                    Worst.CheckType = CheckTypes.BeamLateral;
                }
            }
            // 4. 静态最不利（长细比、局部稳定）
            if (Slenderness.MaxUtil > Worst.MaxUtil)
            {
                Worst.MaxUtil = Slenderness.MaxUtil;
                Worst.CheckType = CheckTypes.Slenderness;
            }

            if (LocalStability.MaxUtil > Worst.MaxUtil)
            {
                Worst.MaxUtil = LocalStability.MaxUtil;
                Worst.CheckType = CheckTypes.LocalStability;
            }
        }


        // 初始化组合字典（传入 member，拿测站列表）
        public void InitCombos(MemberData member, List<string> combos)
        {
            // 拿到所有测站位置
            var stations = member.Stations.Values.ToList();

            foreach (var combo in combos)
            {
                // 强度验算：每个测站都建一个对象
                var flexDict = new Dictionary<double, FlexureSingleResult>();
                var shearDict = new Dictionary<double, ShearSingleResult>();

                foreach (var station in stations)
                {
                    flexDict[station] = new FlexureSingleResult();
                    shearDict[station] = new ShearSingleResult();
                }

                FlexureStrength.Collection[combo] = flexDict;
                ShearStrength.Collection[combo] = shearDict;

                // φb 计算过程
                PhiBResult.Collection[combo] = new PhiBSingleResult();

                // 稳定验算
                InPlaneStability.Collection[combo] = new ColumnInPlaneStabilitySingleResult();
                OutPlaneStability.Collection[combo] = new ColumnOutPlaneStabilitySingleResult();
                LateralStability.Collection[combo] = new BeamLateralStabilitySingleResult();
            }
        }


    }
}
