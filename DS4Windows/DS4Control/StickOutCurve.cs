using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS4Windows.StickModifiers
{
    public static class StickOutCurve
    {
        public enum Curve : uint
        {
            Linear,
            EnhancedPrecision,
            Quadratic,
            Cubic,
            EaseoutQuad,
            EaseoutCubic,
            //Bezier,
        }

        private static SmoothedInput smoothedX = new SmoothedInput(5); // Buffer size 5 for example
        private static SmoothedInput smoothedY = new SmoothedInput(5);

        public static void CalcOutValue(Curve type, double axisXValue, double axisYValue,
            out double axisOutXValue, out double axisOutYValue)
        {
            double smoothedAxisXValue = smoothedX.AddValue(axisXValue);
            double smoothedAxisYValue = smoothedY.AddValue(axisYValue);

            if (type == Curve.Linear)
            {
                axisOutXValue = smoothedAxisXValue;
                axisOutYValue = smoothedAxisYValue;
                return;
            }

            double r = Math.Atan2(axisYValue, axisXValue);
            //Console.WriteLine(r);
            double maxOutXRatio = Math.Abs(Math.Cos(r));
            double maxOutYRatio = Math.Abs(Math.Sin(r));
            double capX = axisXValue >= 0.0 ? maxOutXRatio * 1.0 : maxOutXRatio * 1.0;
            double capY = axisYValue >= 0.0 ? maxOutYRatio * 1.0 : maxOutYRatio * 1.0;
            double absSideX = Math.Abs(axisXValue); double absSideY = Math.Abs(axisYValue);
            if (absSideX > capX) capX = absSideX;
            if (absSideY > capY) capY = absSideY;
            double tempRatioX = capX > 0 ? axisXValue / capX : 0;
            double tempRatioY = capY > 0 ? axisYValue / capY : 0;
            //Console.WriteLine("{0} {1} {2}", axisYValue, tempRatioY, capY);
            double signX = tempRatioX >= 0.0 ? 1.0 : -1.0;
            double signY = tempRatioY >= 0.0 ? 1.0 : -1.0;

            double outputXValue = 0.0;
            double outputYValue = 0.0;
            switch (type)
            {
                case Curve.Linear:
                    outputXValue = axisXValue;
                    outputYValue = axisYValue;
                    break;

                case Curve.EnhancedPrecision:
                    {
                        double absX = Math.Abs(tempRatioX);
                        double absY = Math.Abs(tempRatioY);
                        double tempX, tempY;

                        // More gradual and controlled change in the lower range for maximum precision
                        if (absX <= 0.1) // Extremely gradual in the very low range
                        {
                            tempX = 0.25 * absX; // Very low coefficient for maximum precision
                        }
                        else if (absX <= 0.5) // Gradual change in the lower range
                        {
                            tempX = 0.5 * absX; // Low coefficient for enhanced precision
                        }
                        else if (absX <= 0.75)
                        {
                            tempX = absX - 0.025; // Slightly adjusted offset for more precision
                        }
                        else
                        {
                            tempX = (absX * 1.2) - 0.2; // Adjusted coefficient and offset for maintaining range
                        }

                        outputXValue = signX * tempX * capX;
                        // Similar adjustments for Y axis

                        if (absY <= 0.1) // Extremely gradual in the very low range
                        {
                            tempY = 0.25 * absY; // Very low coefficient for maximum precision
                        }
                        else if (absY <= 0.5) // Gradual change in the lower range
                        {
                            tempY = 0.5 * absY; // Low coefficient for enhanced precision
                        }
                        else if (absY <= 0.75)
                        {
                            tempY = absY - 0.025; // Slightly adjusted offset for more precision
                        }
                        else
                        {
                            tempY = (absY * 1.2) - 0.2; // Adjusted coefficient and offset for maintaining range
                        }

                        outputYValue = signY * tempY * capY;
                    }
                    break;

                case Curve.Quadratic:
                    outputXValue = signX * tempRatioX * tempRatioX * capX;
                    outputYValue = signY * tempRatioY * tempRatioY * capY;
                    break;

                case Curve.Cubic:
                    outputXValue = tempRatioX * tempRatioX * tempRatioX * capX;
                    outputYValue = tempRatioY * tempRatioY * tempRatioY * capY;
                    break;

                case Curve.EaseoutQuad:
                    {
                        double absX = Math.Abs(tempRatioX);
                        double absY = Math.Abs(tempRatioY);
                        double outputX = absX * (absX - 2.0);
                        double outputY = absY * (absY - 2.0);

                        outputXValue = -1.0 * outputX * signX * capX;
                        outputYValue = -1.0 * outputY * signY * capY;
                    }

                    break;

                case Curve.EaseoutCubic:
                    {
                        double innerX = Math.Abs(tempRatioX) - 1.0;
                        double innerY = Math.Abs(tempRatioY) - 1.0;
                        double outputX = innerX * innerX * innerX + 1.0;
                        double outputY = innerY * innerY * innerY + 1.0;

                        outputXValue = 1.0 * outputX * signX * capX;
                        outputYValue = 1.0 * outputY * signY * capY;
                    }

                    break;

                //case Curve.Bezier:
                //    outputXValue = axisXValue * capX;
                //    outputYValue = axisYValue * capY;
                //    break;

                default:
                    outputXValue = axisXValue;
                    outputYValue = axisYValue;
                    break;
            }

            axisOutXValue = outputXValue;
            axisOutYValue = outputYValue;
        }
    }

    private class SmoothedInput
    {
        private readonly Queue<double> _buffer = new Queue<double>();
        private readonly int _bufferSize;
        private double _sum;

        public SmoothedInput(int bufferSize)
        {
            _bufferSize = bufferSize;
        }

        public double AddValue(double value)
        {
            if (_buffer.Count >= _bufferSize)
            {
                _sum -= _buffer.Dequeue();
            }

            _buffer.Enqueue(value);
            _sum += value;

            return _sum / _buffer.Count;
        }
    }
}
