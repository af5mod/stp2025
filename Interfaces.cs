using System;
using System.Collections.Generic;
using System.Drawing;
using DynamicData;
using GraphicEditor.ViewModels;

namespace GraphicEditor
{
    public interface IDrawing
    {
        void DrawLine(PointF a, PointF b);
        void DrawCircle(PointF Center, float r);
    }
    public interface IDrawingFigure
    {
        string DrawingGeometry {get;}
    }

    public interface IFigure : IDrawingFigure
    {
        bool IsSelected { get; set; }

        void Move(PointF vector);
        void Rotate(PointF center, float angle);
        PointF Center { get; set;}
        string Name { get; set; }
        void Scale(float dx, float dy);
        void Scale(PointF center, float dr);
        void Reflection(PointF a, PointF b);
        IFigure Clone();
        void Draw(IDrawing drawing);
        bool IsIn(PointF point, float eps);
        IFigure Intersect(IFigure other);
        IFigure Union(IFigure other);
        IFigure Subtract(IFigure other);
        void SetParameters(IDictionary<string, double> doubleParams, IDictionary<string, PointF> pointParams);
        void RandomizeParameters();
    }

    public interface ILogic
    {
        public void ClearAll();        
        public IObservable<IChangeSet<IFigure, string>> Connect();
        public IObservable<IChangeSet<IFigure>> ConnectSelections();
        IEnumerable<IFigure> Figures { get; } //список всех фигур
        void Save(string FilePath, string FileFormat);
        void Load(string FilePath, string FileFormat);
        IEnumerable<string> FigureNamesToCreate { get; } //список имен фигур доступных для создания
        IEnumerable<(string, Type)> GetParameters(string figure);
        IFigure Create(string name, IDictionary<string, PointF> parameters, IDictionary<string, double> doubleparameters);
        void AddFigure(IFigure figure);
        void RemoveFigure(IFigure figure);
        public void RemoveFigures(IEnumerable<IFigure> figures);
        IFigure? Find(PointF p, float eps);

        void Select(IFigure f, bool multiSelect);
        void UnSelect(IFigure f);
        IEnumerable<IFigure> Selected();
    }
}
