 
/*
    using numpy = Numpy.np;
    using Numpy;
    
    // using V2 = utils_2d.V2;
    
    // using V3 = utils_3d.V3;
    
    // using settings;
    
    using Unity;
    using UnityEngine;
    using System.Linq;
    
    using System.Collections.Generic;
    using System;

    public static class Module {
    private static int BIAS = 0, BIAS_STRENGTH=0,BOUNDARY=0,CLIP=1;

    // Represents and solves the quadratic error function
    public class QEF {
            NDarray A;
            NDarray b;
            float[] fixed_values;
            
            public QEF(NDarray A, NDarray b, float[] fixed_values) {
                this.A = A;
                this.b = b;
                this.fixed_values = fixed_values;
            }
            
            // Evaluates the function at a given point.
            //         This is what the solve method is trying to minimize.
            //         NB: Doesn't work with fixed axes.
            public virtual NDarray evaluate(NDarray x) {
                x = numpy.array(x);
                return numpy.linalg.norm(numpy.matmul(this.A, x) - this.b);
            }
            
            // Evaluates the new QEF at a position, returning the same format solve does.
            public virtual Tuple<NDarray,NDarray> eval_with_pos(NDarray x) {
                return Tuple.Create(this.evaluate(x), x);
            }
            
            // Returns a new QEF that measures the the error from a bunch of normals, each emanating from given positions
            // public static QEF make_2d(NDarray positions, NDarray normals) {
            //     var A = numpy.array(normals);
            //     var b = (from _tup_1 in t.Zip(positions, normals).Chop((v,n) => (v, n))
            //         let v = _tup_1.Item1
            //         let n = _tup_1.Item2
            //         select (v[0] * n[0] + v[1] * n[1])).ToList();
            //     // var fixed_values = new List<object> {
            //     //     null
            //     // } * A.shape[1];
            //     float[] fixed_values =  new float[2];// A.shape[1]; 
            //     return new QEF(A, b, fixed_values);
            // }
            
            // Returns a new QEF that measures the the error from a bunch of normals, each emanating from given positions
            //[staticmethod]
            public static QEF make_3d(List<float> positions, NDarray normals) {
                var A = numpy.array(normals);
                var b = normals;
                // Tuple<NDarray,NDarray> t = new Tuple<NDarray, NDarray>(positions,normals);
                //Enumerable.Zip(positions,normals,t);
                // (Enumerable) t.zip(positions, normals);
                // NDarray cache = new double[]{0,0,0};;
                // var unzip = cache;
                // cache.Zip();
                // Tuple<NDarray,NDarray> t =Tuple.Create(residual, position)

                //https://csharp.hotexamples.com/examples/-/IEnumerable/Zip/php-ienumerable-zip-method-examples.html
                //https://docs.microsoft.com/de-de/dotnet/api/system.linq.enumerable.zip?view=net-5.0

                // var positionsAndNormals = positions.Zip(normals, (p, n) => new { positions = p, normal = n });
                // foreach(var pn in positionsAndNormals)
                // {
                //     // Console.WriteLine(pn.po + nw.Word);
                // }


                // foreach (var tupel in positions.Zip( List<float> normals, Tuple.Create)) 
                //     {
                //         let v = tupel.Item1;
                //         let n = tupel.Item2;
                    
                // var b = (from _tup_1 in t.GetDtype((v,n) => (v, n))
                //     let v = _tup_1.Item1
                //     let n = _tup_1.Item2
                //     select (v[0] * n[0] + v[1] * n[1] + v[2] * n[2])).ToList();
                //     }
                // var fixed_values = new List<object> {
                //     null
                // } * A.shape[1];
                
                float[] fixed_values =  new float[3];// A.shape[1]; 
                return new QEF(A, b, fixed_values);
            }
            
            // Returns a new new QEF that gives the same values as the old one, only with the position along the given axis
            //         constrained to be value.
            public virtual QEF fix_axis(int axis, float value) {
                // Pre-evaluate the fixed axis, adjusting b
                var b = this.b[":"] - this.A[":",axis] * value;
                // Remove that axis from a
                var A = numpy.delete(this.A, axis, 1);
                var fixed_values = this.fixed_values;
                fixed_values[axis] = value;
                return new QEF(A, b, fixed_values);
            }
            
            // Finds the point that minimizes the error of this new QEF,
            //         and returns a tuple of the error squared and the point itself
            public virtual Tuple<NDarray, List<float>> solve() {
                var _tup_1 = numpy.linalg.lstsq(this.A, this.b);
                NDarray result = _tup_1.Item1;
                var residual = _tup_1.Item2;
                var rank = _tup_1.Item3;
                var s = _tup_1.Item4;
                if (residual.size == 0) {
                    residual = this.evaluate(result);
                } else {
                    residual = residual[0];
                }
                // Result only contains the solution for the unfixed axis,
                // we need to add back all the ones we previously fixed.
                var position = new List<float>();
                var i = 0;
                foreach (var value in this.fixed_values) {
                    if (value == null) {
                        position.Add((float)result[i]); //cast maybe wrong!!
                        i += 1;
                    } else {
                        position.Add(value);
                    }
                }
                return Tuple.Create<NDarray, List<float>>(residual, position);
            }
        }
        
        // public static object solve_QEF_2d(object x, object y, object positions, object normals) {
        //     // The error term we are trying to minimize is sum( dot(x-v[i], n[i]) ^ 2)
        //     // This should be minimized over the unit square with top left point (x, y)
        //     // In other words, minimize || A * x - b || ^2 where A and b are a matrix and vector
        //     // derived from v and n
        //     // The heavy lifting is done by the new QEF class, but this function includes some important
        //     // tricks to cope with edge cases
        //     // This is demonstration code and isn't optimized, there are many good C++ implementations
        //     // out there if you need speed.
        //     if (settings.BIAS) {
        //         // Add extra normals that add extra error the further we go
        //         // from the cell, this encourages the final result to be
        //         // inside the cell
        //         // These normals are shorter than the input normals
        //         // as that makes the bias weaker,  we want them to only
        //         // really be important when the input is ambiguous
        //         // Take a simple average of positions as the point we will
        //         // pull towards.
        //         var mass_point = numpy.mean(positions, axis: 0);
        //         normals.Append(new List<object> {
        //             settings.BIAS_STRENGTH,
        //             0
        //         });
        //         positions.Append(mass_point);
        //         normals.Append(new List<object> {
        //             0,
        //             settings.BIAS_STRENGTH
        //         });
        //         positions.Append(mass_point);
        //     }
        //     var new QEF = new QEF.make_2d(positions, normals);
        //     var _tup_1 = new QEF.solve();
        //     var residual = _tup_1.Item1;
        //     var v = _tup_1.Item2;
        //     if (settings.BOUNDARY) {
        //         // It's entirely possible that the best solution to the new QEF is not actually
        //         // inside the cell.
        //         if (!inside((residual, v))) {
        //             // If so, we constrain the the new QEF to the horizontal and vertical
        //             // lines bordering the cell, and find the best point of those
        //             var r1 = new QEF.fix_axis(0, x + 0).solve();
        //             var r2 = new QEF.fix_axis(0, x + 1).solve();
        //             var r3 = new QEF.fix_axis(1, y + 0).solve();
        //             var r4 = new QEF.fix_axis(1, y + 1).solve();
        //             var rs = new List<object> {
        //                 r1,
        //                 r2,
        //                 r3,
        //                 r4
        //             }.Where(inside).ToList().ToList();
        //             if (rs.Count == 0) {
        //                 // It's still possible that those lines (which are infinite)
        //                 // cause solutions outside the box. So finally, we evaluate which corner
        //                 // of the cell looks best
        //                 r1 = new QEF.eval_with_pos((x + 0, y + 0));
        //                 r2 = new QEF.eval_with_pos((x + 0, y + 1));
        //                 r3 = new QEF.eval_with_pos((x + 1, y + 0));
        //                 r4 = new QEF.eval_with_pos((x + 1, y + 1));
        //                 rs = new List<object> {
        //                     r1,
        //                     r2,
        //                     r3,
        //                     r4
        //                 }.Where(inside).ToList().ToList();
        //             }
        //             // Pick the best of the available options
        //             var _tup_2 = min(rs);
        //             residual = _tup_2.Item1;
        //             v = _tup_2.Item2;
        //         }
        //     }
        //     Func<object, object> inside = r => {
        //         return x <= r[1][0] <= x + 1 && y <= r[1][1] <= y + 1;
        //     };
        //     if (settings.CLIP) {
        //         // Crudely force v to be inside the cell
        //         v[0] = numpy.clip(v[0], x, x + 1);
        //         v[1] = numpy.clip(v[1], y, y + 1);
        //     }
        //     return new Vector2(v[0], v[1]);
        // }
        
        public static object solve_QEF_3d(
            float x,
            float y,
            float z,
            List<float> positions,
            NDarray normals) {
            object r8;
            object r7;
            // The error term we are trying to minimize is sum( dot(x-v[i], n[i]) ^ 2)
            // This should be minimized over the unit square with top left point (x, y)
            // In other words, minimize || A * x - b || ^2 where A and b are a matrix and vector
            // derived from v and n
            // The heavy lifting is done by the new QEF class, but this function includes some important
            // tricks to cope with edge cases
            // This is demonstration code and isn't optimized, there are many good C++ implementations
            // out there if you need speed.
            if (BIAS ==1 ) {
                // Add extra normals that add extra error the further we go
                // from the cell, this encourages the final result to be
                // inside the cell
                // These normals are shorter than the input normals
                // as that makes the bias weaker,  we want them to only
                // really be important when the input is ambiguous
                // Take a simple average of positions as the point we will
                // pull towards.
                var mass_point = positions.Average();//Math. numpy.mean(positions, axis: 0);
                NDarray a = new double[]{BIAS_STRENGTH,0,0};
                normals.append( a);
                positions.Add(mass_point);
                NDarray b = new double[]{0,BIAS_STRENGTH,0};
                normals.append(b);
                positions.Add(mass_point);
                NDarray c = new double[]{0,0,BIAS_STRENGTH};
                normals.append(c);
                positions.Add(mass_point);
            }
            var QEFL = QEF.make_3d(positions, normals);
            var _tup_1 = QEFL.solve();
            var residual = _tup_1.Item1;
            List<float> v = _tup_1.Item2;
              
            if (BOUNDARY ==1 ) {
                
            /* Func<float, float> inside = r => {
            //     return x <= r[1][0] <= x + 1 && y <= r[1][1] <= y + 1 && z <= r[1][2] <= z + 1;
            // };
                // It's entirely possible that the best solution to the  QEF is not actually
                // inside the cell.
                // if (!inside((residual, v))) {
                //     // If so, we constrain the the new QEF to the 6
                //     // planes bordering the cell, and find the best point of those
                //     QEFL.fix_axis(0, x + 0);
                //     var r1 =  QEFL.solve();

                //     QEFL.fix_axis(0, x + 1);
                //     var r2 =  QEFL.solve();
                //     QEFL.fix_axis(1, y + 0);
                //     var r3 =  QEFL.solve();
                //     QEFL.fix_axis(1, y + 1);
                //     var r4 =  QEFL.solve();
                //     QEFL.fix_axis(2, z + 0);
                //     var r5 =  QEFL.solve();
                //     QEFL.fix_axis(2, z + 1);
                //     var r6 =  QEFL.solve();
                //     var rs =  List<float> {
                //         r1,
                //         r2,
                //         r3,
                //         r4,
                //         r5,
                //         r6
                //     }.Where(inside).ToList().ToList();
                //     if (rs.Count == 0) {
                //         // It's still possible that those planes (which are infinite)
                //         // cause solutions outside the box.
                //         // So now try the 12 lines bordering the cell
                //         r1 = Qef.fix_axis(1, y + 0).fix_axis(0, x + 0).solve();
                //         r2 = QEF.fix_axis(1, y + 1).fix_axis(0, x + 0).solve();
                //         r3 = QEF.fix_axis(1, y + 0).fix_axis(0, x + 1).solve();
                //         r4 = QEF.fix_axis(1, y + 1).fix_axis(0, x + 1).solve();
                //         r5 = QEF.fix_axis(2, z + 0).fix_axis(0, x + 0).solve();
                //         r6 = QEF.fix_axis(2, z + 1).fix_axis(0, x + 0).solve();
                //         r7 = QEF.fix_axis(2, z + 0).fix_axis(0, x + 1).solve();
                //         r8 = QEF.fix_axis(2, z + 1).fix_axis(0, x + 1).solve();
                //         var r9 = QEF.fix_axis(2, z + 0).fix_axis(1, y + 0).solve();
                //         var r10 = QEF.fix_axis(2, z + 1).fix_axis(1, y + 0).solve();
                //         var r11 = QEF.fix_axis(2, z + 0).fix_axis(1, y + 1).solve();
                //         var r12 = QEF.fix_axis(2, z + 1).fix_axis(1, y + 1).solve();
                //         rs = new List<object> {
                //             r1,
                //             r2,
                //             r3,
                //             r4,
                //             r5,
                //             r6,
                //             r7,
                //             r8,
                //             r9,
                //             r10,
                //             r11,
                //             r12
                //         }.Where(inside).ToList().ToList();
                //     }
                //     if (rs.Count == 0) {
                //         // So finally, we evaluate which corner
                //         // of the cell looks best
                //         r1 = new QEF.eval_with_pos((x + 0, y + 0, z + 0));
                //         r2 = new QEF.eval_with_pos((x + 0, y + 0, z + 1));
                //         r3 = new QEF.eval_with_pos((x + 0, y + 1, z + 0));
                //         r4 = new QEF.eval_with_pos((x + 0, y + 1, z + 1));
                //         r5 = new QEF.eval_with_pos((x + 1, y + 0, z + 0));
                //         r6 = new QEF.eval_with_pos((x + 1, y + 0, z + 1));
                //         r7 = new QEF.eval_with_pos((x + 1, y + 1, z + 0));
                //         r8 = new QEF.eval_with_pos((x + 1, y + 1, z + 1));
                //         rs = new List<object> {
                //             r1,
                //             r2,
                //             r3,
                //             r4,
                //             r5,
                //             r6,
                //             r7,
                //             r8
                //         }.Where(inside).ToList().ToList();
                //     }
                //     // Pick the best of the available options
                //     var _tup_2 = min(rs);
                //     residual = _tup_2.Item1;
                //     v = _tup_2.Item2;
                // }
                *
            }
            if (CLIP==1) {
                // Crudely force v to be inside the cell
                
                v[0] = Mathf.Clamp(v[0], x, x + 1);
                v[0] = Mathf.Clamp(v[0], x, x + 1);
                v[1] = Mathf.Clamp(v[1], y, y + 1);
                v[2] = Mathf.Clamp(v[2], z, z + 1);
            }
            return new Vector3(v[0], v[1], v[2]);
        }
    }
    */