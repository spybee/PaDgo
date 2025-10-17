using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PaDgo
{
    public static class TrainingMode
    {
        public static void StartTraining(OrbType currentOrbType)
        {
            Debug.Print($"開始訓練模式: {currentOrbType}");
            Debug.Print("請截取該類型的寶珠，程序會自動收集顏色樣本...");
        }

        public static void AddTrainingSampleFromUser(OrbType orbType, Bitmap bmp, Point point)
        {
            var averageColor = AdvancedOrbRecognizer.GetOrbAverageColor(bmp, point);
            ColorRangeTrainer.AddTrainingSample(orbType, averageColor);

            Debug.Print($"已添加訓練樣本: {orbType} - {averageColor}");
        }

        public static void FinishTraining()
        {
            var newProfiles = ColorRangeTrainer.GenerateColorProfilesFromTraining();
            ColorRangeTrainer.SaveTrainingData("orb_training_data.json");
            Debug.Print("訓練完成！新的顏色範圍已生成並保存。");
        }
    }
}
