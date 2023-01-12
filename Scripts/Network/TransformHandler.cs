using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Sfs2X.Entities.Data;

namespace Multiplayer.Smartfox.Network
{
    public class TransformHandler
    {
        private TransformHandler() { }
        private Vector3 position; // Position as Vector3
        private Vector3 angleRotation; // Rotation as 3 euler angles - x, y, z
        private double timeStamp = 0;

        public Vector3 Position
        {
            get
            {
                return position;
            }
        }

        public Vector3 AngleRotation
        {
            get
            {
                return angleRotation;
            }
        }

        /**
        * <summary>
        * Returns the rotation with y component only
        * </summary>
        */
        public Vector3 AngleRotationFPS
        {
            get
            {
                return new Vector3(0, angleRotation.y, 0);
            }
        }

        public Quaternion Rotation
        {
            get
            {
                return Quaternion.Euler(AngleRotationFPS);
            }
        }

        /**
        * <summary>
        * Timestamp needed for interpolation and extrapolation
        * </summary>
        */
        public double TimeStamp
        {
            get
            {
                return timeStamp;
            }
            set
            {
                timeStamp = value;
            }
        }

        /**
        * <summary>
        * Stores the transform values to SFSObject to send them to server
        * </summary>
        */
        public void ToSFSObject(ISFSObject data)
        {
            ISFSObject tr = new SFSObject();

            tr.PutDouble("x", Convert.ToDouble(this.position.x));
            tr.PutDouble("y", Convert.ToDouble(this.position.y));
            tr.PutDouble("z", Convert.ToDouble(this.position.z));

            tr.PutDouble("rx", Convert.ToDouble(this.angleRotation.x));
            tr.PutDouble("ry", Convert.ToDouble(this.angleRotation.y));
            tr.PutDouble("rz", Convert.ToDouble(this.angleRotation.z));

            tr.PutLong("t", Convert.ToInt64(this.timeStamp));

            data.PutSFSObject("transform", tr);
        }

        /**
        * <summary>
        * Creating NetworkTransform from SFS object
        * </summary>
        */
        public static TransformHandler FromSFSObject(ISFSObject data) {
            TransformHandler trans = new();
            ISFSObject transformData = data.GetSFSObject("transform");

            float x = Convert.ToSingle(transformData.GetDouble("x"));
            float y = Convert.ToSingle(transformData.GetDouble("y"));
            float z = Convert.ToSingle(transformData.GetDouble("z"));

            float rx = Convert.ToSingle(transformData.GetDouble("rx"));
            float ry = Convert.ToSingle(transformData.GetDouble("ry"));
            float rz = Convert.ToSingle(transformData.GetDouble("rz"));

            trans.position = new Vector3(x, y, z);
            trans.angleRotation = new Vector3(rx, ry, rz);

            if (transformData.ContainsKey("t")) {
                trans.TimeStamp = Convert.ToDouble(transformData.GetLong("t"));
            }
            else {
                trans.TimeStamp = 0;
            }

            return trans;
        }
	
        // Creating NetworkTransform from Unity transform
        public static TransformHandler FromTransform(Transform transform)
        {
            TransformHandler trans = new()
            {
                position = transform.position,
                angleRotation = transform.localEulerAngles
            };

            return trans;
        }
    }
}