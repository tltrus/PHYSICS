using System;
using System.Collections.Generic;
using System.Windows;
using DrawingVisualApp.Models;

namespace DrawingVisualApp.Services
{
    /// <summary>
    /// Физический движок на основе Verlet интеграции
    /// </summary>
    public class PhysicsEngine
    {
        private List<VerletPoint> _points;
        private List<Stick> _sticks;
        private double _canvasWidth;
        private double _canvasHeight;

        // Состояние перетаскивания платформы
        private bool _isDraggingPlatform = false;
        private Point _lastMousePosition;
        private Vector _platformDelta;

        // Платформа
        public Rect Platform { get; set; }

        public PhysicsEngine(List<VerletPoint> points, List<Stick> sticks,
                           double canvasWidth, double canvasHeight)
        {
            _points = points;
            _sticks = sticks;
            _canvasWidth = canvasWidth;
            _canvasHeight = canvasHeight;
        }

        #region Управление платформой

        /// <summary>
        /// Начинает перетаскивание платформы
        /// </summary>
        public bool StartPlatformDrag(Point mousePosition)
        {
            if (Platform.Contains(mousePosition))
            {
                _isDraggingPlatform = true;
                _lastMousePosition = mousePosition;
                _platformDelta = new Vector();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Обновляет позицию платформы при перетаскивании
        /// </summary>
        public void UpdatePlatformDrag(Point mousePosition)
        {
            if (!_isDraggingPlatform) return;

            // Вычисляем смещение мыши
            _platformDelta = mousePosition - _lastMousePosition;
            _lastMousePosition = mousePosition;

            // Перемещаем платформу
            Platform = new Rect(
                Platform.X + _platformDelta.X,
                Platform.Y,
                Platform.Width,
                Platform.Height
            );

            // Немедленно перемещаем точки на платформе
            MovePointsWithPlatform();
        }

        /// <summary>
        /// Завершает перетаскивание платформы
        /// </summary>
        public void StopPlatformDrag()
        {
            _isDraggingPlatform = false;
            _platformDelta = new Vector();
        }

        /// <summary>
        /// Перемещает точки, находящиеся на платформе
        /// </summary>
        private void MovePointsWithPlatform()
        {
            if (_platformDelta.Length == 0) return;

            foreach (var point in _points)
            {
                if (IsPointOnPlatform(point))
                {
                    // Синхронное перемещение точки с платформой
                    point.Position += _platformDelta;
                    point.PreviousPosition += _platformDelta;
                }
            }
        }

        /// <summary>
        /// Проверяет, находится ли точка на платформе
        /// </summary>
        private bool IsPointOnPlatform(VerletPoint point)
        {
            // Учитываем радиус точки при проверке
            double pointRadius = Constants.PointRadius;

            // Проверяем по оси X: точка должна быть над платформой (с учетом радиуса)
            bool isOverPlatformX = point.Position.X >= Platform.Left - pointRadius &&
                                   point.Position.X <= Platform.Right + pointRadius;

            if (!isOverPlatformX) return false;

            // Проверяем по оси Y: точка должна быть очень близко к верху платформы
            double verticalDistance = point.Position.Y - Platform.Top;

            // Точка считается "на платформе", если она находится в пределах contactEpsilon
            // выше или ниже верха платформы
            return verticalDistance >= -Constants.ContactEpsilon &&
                   verticalDistance <= Constants.ContactEpsilon;
        }

        #endregion

        #region Обработка коллизий

        /// <summary>
        /// Обрабатывает коллизии точек с платформой
        /// </summary>
        public void HandlePlatformCollisions()
        {
            foreach (var point in _points)
            {
                // Пропускаем точки, которые не находятся над платформой по горизонтали
                if (!IsPointOverPlatform(point)) continue;

                double pointRadius = Constants.PointRadius;
                double distanceToTop = point.Position.Y - Platform.Top;

                // Если точка находится в зоне контакта с платформой
                if (distanceToTop >= -pointRadius && distanceToTop <= Constants.ContactEpsilon)
                {
                    ResolvePlatformCollision(point, distanceToTop);
                }
            }
        }

        /// <summary>
        /// Проверяет, находится ли точка над платформой по горизонтали
        /// </summary>
        private bool IsPointOverPlatform(VerletPoint point)
        {
            double pointRadius = Constants.PointRadius;

            // Учитываем радиус точки при проверке
            return point.Position.X >= Platform.Left - pointRadius &&
                   point.Position.X <= Platform.Right + pointRadius;
        }

        /// <summary>
        /// Разрешает коллизию точки с платформой
        /// </summary>
        private void ResolvePlatformCollision(VerletPoint point, double distanceToTop)
        {
            Vector velocity = point.Position - point.PreviousPosition;

            // Если точка на уровне или ниже платформы (с небольшим допуском)
            if (distanceToTop >= 0)
            {
                // Поднимаем точку на верх платформы
                point.Position = new Point(point.Position.X, Platform.Top);

                // Применяем трение платформы - просто умножаем горизонтальную скорость на коэффициент
                velocity.X *= Constants.PlatformFriction;

                // Отскок от платформы
                if (velocity.Y > 0)
                {
                    velocity.Y = -velocity.Y * 0.3; // Коэффициент упругости
                }
                else
                {
                    velocity.Y = 0; // Если точка уже двигалась вверх, убираем вертикальную скорость
                }

                point.PreviousPosition = point.Position - velocity;
            }
            // Если точка чуть выше платформы (в пределах радиуса точки)
            else if (distanceToTop > -Constants.PointRadius * 2)
            {
                // Мягкая коррекция: плавно притягиваем к платформе
                double correctionFactor = 0.3;
                double targetY = Platform.Top - distanceToTop * correctionFactor;
                point.Position = new Point(point.Position.X, targetY);
            }
        }

        /// <summary>
        /// Обрабатывает столкновения с границами мира
        /// </summary>
        public void HandleWorldBoundaries()
        {
            double minX = Constants.PointRadius;
            double minY = Constants.PointRadius;
            double maxX = _canvasWidth - Constants.PointRadius;
            double maxY = _canvasHeight - Constants.PointRadius;

            foreach (var point in _points)
            {
                Vector velocity = point.Position - point.PreviousPosition;

                // Столкновение с левой стеной
                if (point.Position.X < minX)
                {
                    ResolveWallCollision(point, velocity, true, minX);
                }
                // Столкновение с правой стеной
                else if (point.Position.X > maxX)
                {
                    ResolveWallCollision(point, velocity, true, maxX);
                }

                // Столкновение с потолком
                if (point.Position.Y < minY)
                {
                    ResolveWallCollision(point, velocity, false, minY);
                }
                // Столкновение с полом
                else if (point.Position.Y > maxY)
                {
                    ResolveWallCollision(point, velocity, false, maxY);
                }
            }
        }

        /// <summary>
        /// Разрешает столкновение со стеной
        /// </summary>
        private void ResolveWallCollision(VerletPoint point, Vector velocity,
                                        bool isHorizontal, double boundary)
        {
            if (isHorizontal)
            {
                point.Position = new Point(boundary, point.Position.Y);
                velocity.X *= -Constants.BounceFactor;
                velocity.Y *= Constants.WallFriction;
            }
            else
            {
                point.Position = new Point(point.Position.X, boundary);
                velocity.Y *= -Constants.BounceFactor;
                velocity.X *= Constants.WallFriction;
            }

            UpdatePreviousPosition(point, velocity);
        }

        /// <summary>
        /// Обновляет предыдущую позицию точки после столкновения
        /// </summary>
        private void UpdatePreviousPosition(VerletPoint point, Vector velocity)
        {
            Vector positionAsVector = (Vector)point.Position;
            Vector newPrevPos = positionAsVector - velocity;
            point.PreviousPosition = new Point(newPrevPos.X, newPrevPos.Y);
        }

        #endregion

        #region Основной цикл физики

        /// <summary>
        /// Обновляет физическую симуляцию на один кадр
        /// </summary>
        public void UpdatePhysics(bool applyGravity)
        {
            // Применяем внешние силы и обновляем позиции
            ApplyForcesAndUpdatePoints(applyGravity);

            // Решаем ограничения и коллизии
            ResolveConstraintsAndCollisions();
        }

        /// <summary>
        /// Применяет силы и обновляет точки
        /// </summary>
        private void ApplyForcesAndUpdatePoints(bool applyGravity)
        {
            foreach (var point in _points)
            {
                if (applyGravity)
                    point.ApplyGravity(Constants.Gravity);

                point.Update(Constants.Damping);
            }
        }

        /// <summary>
        /// Решает ограничения и обрабатывает коллизии
        /// </summary>
        private void ResolveConstraintsAndCollisions()
        {
            for (int iteration = 0; iteration < Constants.ConstraintIterations; iteration++)
            {
                // Обрабатываем связи в первую очередь
                if ((iteration & 1) == 0)
                {
                    // Чётная итерация — прямой порядок
                    for (int i = 0; i < _sticks.Count; i++)
                        _sticks[i].Satisfy();
                }
                else
                {
                    // Нечётная итерация — обратный порядок
                    for (int i = _sticks.Count - 1; i >= 0; i--)
                        _sticks[i].Satisfy();
                }

                // Затем обрабатываем коллизии
                HandleWorldBoundaries();

                // Обрабатываем коллизии с платформой только на последней итерации
                if (iteration == Constants.ConstraintIterations - 1)
                {
                    HandlePlatformCollisions();
                }
            }

            // Сбрасываем дельту платформы, если не перетаскиваем
            if (!_isDraggingPlatform)
            {
                _platformDelta = new Vector();
            }
        }

        #endregion

        #region Эффекты

        /// <summary>
        /// Создает взрыв в указанной точке
        /// </summary>
        public void CreateExplosion(Point center, List<VerletPoint> points, List<Stick> sticks)
        {
            // Удаляем связи в радиусе взрыва
            RemoveSticksInExplosionRadius(center, sticks);

            // Применяем импульс к точкам
            ApplyExplosionImpulse(center, points);
        }

        /// <summary>
        /// Удаляет связи в радиусе взрыва
        /// </summary>
        private void RemoveSticksInExplosionRadius(Point center, List<Stick> sticks)
        {
            sticks.RemoveAll(stick =>
            {
                double distanceA = (stick.PointA.Position - center).Length;
                double distanceB = (stick.PointB.Position - center).Length;
                return distanceA < 120 || distanceB < 120;
            });
        }

        /// <summary>
        /// Применяет импульс взрыва к точкам
        /// </summary>
        private void ApplyExplosionImpulse(Point center, List<VerletPoint> points)
        {
            foreach (var point in points)
            {
                Vector direction = point.Position - center;
                double length = direction.Length;

                if (length > 1 && length < 140)
                {
                    direction.Normalize();
                    point.ApplyImpulse(direction * 10);
                }
            }
        }

        #endregion
    }
}