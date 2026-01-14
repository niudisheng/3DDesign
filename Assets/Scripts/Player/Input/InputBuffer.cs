using System.Collections.Generic;
using UnityEngine;

public class InputBuffer
{
    private Dictionary<InputIntent, BufferedInput> buffer
        = new();

    /// <summary>
    /// 记录输入意图
    /// </summary>
    /// <param name="intent"></param>
    public void Record(InputIntent intent)
    {
        buffer[intent] = new BufferedInput
        {
            pressed = true,
            time = Time.time
        };
    }

    /// <summary>
    /// 检测是否在这个时间段内有该输入意图，并消费掉它
    /// </summary>
    /// <param name="intent"></param>
    /// <param name="bufferTime"></param>
    /// <returns></returns>
    public bool TryConsume(InputIntent intent, float bufferTime)
    {
        if (!buffer.TryGetValue(intent, out var input))
            return false;

        if (Time.time - input.time > bufferTime)
            return false;

        buffer[intent] = default;
        return true;
    }
}