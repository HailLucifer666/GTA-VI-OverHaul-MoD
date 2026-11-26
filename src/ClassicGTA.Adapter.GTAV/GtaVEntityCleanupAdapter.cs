using ClassicGTA.Contracts;
using GTA;

namespace ClassicGTA.Adapter.GTAV
{
    internal sealed class GtaVEntityCleanupAdapter : IEntityCleanupAdapter
    {
        public void DeleteEntity(int entityHandle)
        {
            Entity entity = Entity.FromHandle(entityHandle);
            if (entity != null && entity.Exists())
            {
                entity.Delete();
            }
        }

        public void MarkEntityNoLongerNeeded(int entityHandle)
        {
            Entity entity = Entity.FromHandle(entityHandle);
            if (entity != null && entity.Exists())
            {
                entity.MarkAsNoLongerNeeded();
            }
        }

        public void DeleteBlip(int blipHandle)
        {
            var blip = new Blip(blipHandle);
            if (blip.Exists())
            {
                blip.Delete();
            }
        }

        public void RestoreRenderingCamera()
        {
            World.RenderingCamera = null;
        }

        public void DeleteCamera(int cameraHandle)
        {
            var cam = new Camera(cameraHandle);
            if (cam.Exists())
            {
                cam.Delete();
            }
        }
    }
}
