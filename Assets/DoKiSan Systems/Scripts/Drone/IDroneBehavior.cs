public interface IDroneBehavior
{
    /// <summary>Инициализация поведения (вызов перед StartMission)</summary>
    void Init(DroneBase drone);

    /// <summary>Вызывается из Update дрона — поведение должно управлять drone (SetTarget/Stop/и т.д.)</summary>
    void Tick(float dt);

    /// <summary>Готов ли поведение (миссия завершена)</summary>
    bool IsCompleted { get; }

    /// <summary>Опционально: вызвать при отмене/удалении</summary>
    void OnAbort();
}
