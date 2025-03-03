using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace RTS {
    public static class LoadManager {

        public static void LoadGame(string filename) {
            char separator = Path.DirectorySeparatorChar;
            string path = "SavedGames" + separator + PlayerManager.GetPlayerName() + separator + filename + ".json";
            if (!File.Exists(path)) {
                Debug.Log("Unable to find " + path + ". Loading will crash, so aborting.");
                return;
            }
            string input;
            using (StreamReader sr = new(path)) {
                input = sr.ReadToEnd();
            }
            if (input != null) {
                //parse contents of file
                using JsonTextReader reader = new(new StringReader(input));
                while (reader.Read()) {
                    if (reader.Value != null) {
                        if (reader.TokenType == JsonToken.PropertyName) {
                            string property = (string)reader.Value;
                            switch (property) {
                            case "Sun": LoadLighting(reader); break;
                            case "Ground": LoadTerrain(reader); break;
                            case "Camera": LoadCamera(reader); break;
                            case "Resources": LoadResources(reader); break;
                            case "Players": LoadPlayers(reader); break;
                            default: break;
                            }
                        }
                    }
                }
            }
        }

        private static void LoadLighting(JsonTextReader reader) {
            if (reader == null) return;
            Vector3 position = new(0, 0, 0), scale = new(1, 1, 1);
            Quaternion rotation = new(0, 0, 0, 0);
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) {
                        if ((string)reader.Value == "Position") position = LoadVector(reader);
                        else if ((string)reader.Value == "Rotation") rotation = LoadQuaternion(reader);
                        else if ((string)reader.Value == "Scale") scale = LoadVector(reader);
                    }
                } else if (reader.TokenType == JsonToken.EndObject) {
                    GameObject sun = Object.Instantiate(ResourceManager.GetWorldObject("Sun"), position, rotation);
                    sun.transform.localScale = scale;
                    return;
                }
            }
        }

        private static void LoadTerrain(JsonTextReader reader) {
            if (reader == null) return;
            Vector3 position = new(0, 0, 0), scale = new(1, 1, 1);
            Quaternion rotation = new(0, 0, 0, 0);
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) {
                        if ((string)reader.Value == "Position") position = LoadVector(reader);
                        else if ((string)reader.Value == "Rotation") rotation = LoadQuaternion(reader);
                        else if ((string)reader.Value == "Scale") scale = LoadVector(reader);
                    }
                } else if (reader.TokenType == JsonToken.EndObject) {
                    GameObject ground = Object.Instantiate(ResourceManager.GetWorldObject("Ground"), position, rotation);
                    ground.transform.localScale = scale;
                    return;
                }
            }
        }

        private static void LoadCamera(JsonTextReader reader) {
            if (reader == null) return;
            Vector3 position = new(0, 0, 0), scale = new(1, 1, 1);
            Quaternion rotation = new(0, 0, 0, 0);
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) {
                        if ((string)reader.Value == "Position") position = LoadVector(reader);
                        else if ((string)reader.Value == "Rotation") rotation = LoadQuaternion(reader);
                        else if ((string)reader.Value == "Scale") scale = LoadVector(reader);
                    }
                } else if (reader.TokenType == JsonToken.EndObject) {
                    GameObject camera = Camera.main.gameObject;
                    camera.transform.SetLocalPositionAndRotation(position, rotation);
                    camera.transform.localScale = scale;
                    return;
                }
            }
        }

        private static void LoadResources(JsonTextReader reader) {
            if (reader == null) return;
            string currValue = "", type = "";
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) currValue = (string)reader.Value;
                    else if (currValue == "Type") {
                        type = (string)reader.Value;
                        GameObject newObject = Object.Instantiate(ResourceManager.GetWorldObject(type));
                        Resource resource = newObject.GetComponent<Resource>();
                        resource.LoadDetails(reader);
                    }
                } else if (reader.TokenType == JsonToken.EndArray) return;
            }
        }

        private static void LoadPlayers(JsonTextReader reader) {
            if (reader == null) return;
            while (reader.Read()) {
                if (reader.TokenType == JsonToken.StartObject) {
                    GameObject newObject = Object.Instantiate(ResourceManager.GetPlayerObject());
                    Player player = newObject.GetComponent<Player>();
                    player.LoadDetails(reader);
                } else if (reader.TokenType == JsonToken.EndArray) return;
            }
        }

        public static Color LoadColor(JsonTextReader reader) {
            if (reader == null) return default;
            Color color = new(0, 0, 0, 0);
            string currVal = "";
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) currVal = (string)reader.Value;
                    else {
                        switch (currVal) {
                        case "r": color.r = (float)(double)reader.Value; break;
                        case "g": color.g = (float)(double)reader.Value; break;
                        case "b": color.b = (float)(double)reader.Value; break;
                        case "a": color.a = (float)(double)reader.Value; break;
                        default: break;
                        }
                    }
                } else if (reader.TokenType == JsonToken.EndObject) return color;
            }
            return color;
        }

        public static Vector3 LoadVector(JsonTextReader reader) {
            Vector3 position = new(0, 0, 0);
            if (reader == null) return position;
            string currVal = "";
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) currVal = (string)reader.Value;
                    else {
                        switch (currVal) {
                        case "x": position.x = (float)(double)reader.Value; break;
                        case "y": position.y = (float)(double)reader.Value; break;
                        case "z": position.z = (float)(double)reader.Value; break;
                        default: break;
                        }
                    }
                } else if (reader.TokenType == JsonToken.EndObject) return position;
            }
            return position;
        }

        public static Quaternion LoadQuaternion(JsonTextReader reader) {
            Quaternion rotation = new(0, 0, 0, 0);
            if (reader == null) return rotation;
            string currVal = "";
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) currVal = (string)reader.Value;
                    else {
                        switch (currVal) {
                        case "x": rotation.x = (float)(double)reader.Value; break;
                        case "y": rotation.y = (float)(double)reader.Value; break;
                        case "z": rotation.z = (float)(double)reader.Value; break;
                        case "w": rotation.w = (float)(double)reader.Value; break;
                        default: break;
                        }
                    }
                } else if (reader.TokenType == JsonToken.EndObject) return rotation;
            }
            return rotation;
        }

        public static List<string> LoadStringArray(JsonTextReader reader) {
            List<string> values = new();
            while (reader.Read()) {
                if (reader.Value != null) {
                    values.Add((string)reader.Value);
                } else if (reader.TokenType == JsonToken.EndArray) return values;
            }
            return values;
        }

        public static Rect LoadRect(JsonTextReader reader) {
            Rect rect = new(0, 0, 0, 0);
            if (reader == null) return rect;
            string currVal = "";
            while (reader.Read()) {
                if (reader.Value != null) {
                    if (reader.TokenType == JsonToken.PropertyName) currVal = (string)reader.Value;
                    else {
                        switch (currVal) {
                        case "x": rect.x = (float)(double)reader.Value; break;
                        case "y": rect.y = (float)(double)reader.Value; break;
                        case "width": rect.width = (float)(double)reader.Value; break;
                        case "height": rect.height = (float)(double)reader.Value; break;
                        default: break;
                        }
                    }
                } else if (reader.TokenType == JsonToken.EndObject) return rect;
            }
            return rect;
        }
    }
}
