using System;
using Avalonia;

namespace Solitaire.Controls;

internal sealed class CardMotionCurve
{
    private readonly CardPose _start;
    private readonly CardPose _end;
    private readonly CardPoseVelocity _velocity;
    private readonly double _duration;
    internal double Duration => _duration;
    private readonly bool _held;
    private readonly bool _easeOut;
    private readonly double _arch;

    public CardMotionCurve(CardPose start, CardMotionTarget target, CardPoseVelocity? velocity,
        double duration, bool held, bool easeOut)
    {
        _start = start;
        _end = new CardPose(target.Position, target.Turn, held ? 24 : 0);
        _duration = duration;
        _held = held;
        _easeOut = easeOut;
        _arch = velocity is null && !held && start.Lift == 0 && start.Position != target.Position ? target.Deal ? 5 : 1.5 : 0;
        var rate = (easeOut ? 3 : 0) / duration;
        _velocity = velocity ?? new CardPoseVelocity((_end.Position - start.Position) * rate,
            (_end.Turn - start.Turn) * rate, (_end.Lift - start.Lift + _arch * Math.PI) * rate);
    }

    public CardPose Sample(double progress)
    {
        var x = Channel(_start.Position.X, _end.Position.X, _velocity.Position.X, progress);
        var y = Channel(_start.Position.Y, _end.Position.Y, _velocity.Position.Y, progress);
        var turn = Turn(progress);
        var lift = Lift(progress);
        return new CardPose(_held ? _start.Position : new Vector(x.Value, y.Value), turn.Value, lift.Value);
    }

    public CardPoseVelocity Velocity(double progress)
    {
        var x = Channel(_start.Position.X, _end.Position.X, _velocity.Position.X, progress);
        var y = Channel(_start.Position.Y, _end.Position.Y, _velocity.Position.Y, progress);
        return new CardPoseVelocity(_held ? default : new Vector(x.Velocity, y.Velocity),
            Turn(progress).Velocity, Lift(progress).Velocity);
    }

    private ChannelSample Turn(double progress) =>
        Bound(Channel(_start.Turn, _end.Turn, _velocity.Turn, progress, 0, 1), 0, 1);

    private ChannelSample Lift(double progress)
    {
        if (progress >= 1)
            return new ChannelSample(_end.Lift, 0);
        var rate = (_easeOut ? 3 : 0) / _duration;
        var lift = Channel(_start.Lift, _end.Lift, _velocity.Lift - _arch * Math.PI * rate, progress, 0);
        var ease = _easeOut ? 1 - Math.Pow(1 - progress, 3) : progress * progress * (3 - 2 * progress);
        var derivative = _easeOut ? 3 * (1 - progress) * (1 - progress) : 6 * progress * (1 - progress);
        return Bound(new ChannelSample(lift.Value + _arch * Math.Sin(ease * Math.PI),
            lift.Velocity + _arch * Math.PI * Math.Cos(ease * Math.PI) * derivative / _duration), 0, double.PositiveInfinity);
    }

    private ChannelSample Channel(double start, double end, double velocity, double progress,
        double minimum = double.NegativeInfinity, double maximum = double.PositiveInfinity)
    {
        var duration = _duration;
        // For a nearby destination, stop sooner to avoid overshoot.
        if (velocity * (end - start) > 0)
            duration = Math.Min(duration, Math.Max(.000001, 3 * Math.Abs((end - start) / velocity)));
        // A reversed turn keeps its speed within the card limits.
        if (velocity > 0 && double.IsFinite(maximum))
            duration = Math.Min(duration, Math.Max(.000001, 3 * (maximum - start) / velocity));
        else if (velocity < 0 && double.IsFinite(minimum))
            duration = Math.Min(duration, Math.Max(.000001, 3 * (minimum - start) / velocity));
        var time = Math.Clamp(progress * _duration / duration, 0, 1);
        if (time >= 1)
            return new ChannelSample(end, 0);
        var square = time * time;
        var cube = square * time;
        return new ChannelSample(start + (end - start) * (3 * square - 2 * cube) + velocity * duration * (cube - 2 * square + time),
            (end - start) * (6 * time - 6 * square) / duration + velocity * (3 * square - 4 * time + 1));
    }

    private static ChannelSample Bound(ChannelSample sample, double minimum, double maximum)
    {
        var velocity = sample.Value <= minimum && sample.Velocity < 0 || sample.Value >= maximum && sample.Velocity > 0
            ? 0 : sample.Velocity;
        return new ChannelSample(Math.Clamp(sample.Value, minimum, maximum), velocity);
    }

    private readonly record struct ChannelSample(double Value, double Velocity);
}
